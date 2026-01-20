using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SyndicationClient.Authentication;
using SyndicationClient.Caching;
using SyndicationClient.Models;

namespace SyndicationClient.Http;

/// <summary>
/// HTTP client for fetching feed content with caching support.
/// </summary>
public class FeedHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IFeedCache _cache;
    private readonly SyndicationClientOptions _options;
    private readonly ILogger<FeedHttpClient> _logger;

    public FeedHttpClient(
        HttpClient httpClient,
        IFeedCache cache,
        IOptions<SyndicationClientOptions> options,
        ILogger<FeedHttpClient> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Fetches content from the specified URL.
    /// </summary>
    public async Task<FeedHttpResponse> GetAsync(
        string url,
        FeedCredentials? credentials,
        CancellationToken cancellationToken)
    {
        var uri = new Uri(url);
        return await GetAsync(uri, credentials, cancellationToken);
    }

    /// <summary>
    /// Fetches content from the specified URI.
    /// </summary>
    public async Task<FeedHttpResponse> GetAsync(
        Uri uri,
        FeedCredentials? credentials,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Fetching feed from {Url}", uri);

        var request = new HttpRequestMessage(HttpMethod.Get, uri);

        // Add User-Agent
        request.Headers.UserAgent.ParseAdd(_options.UserAgent);

        // Add authentication
        ApplyCredentials(request, credentials);

        // Add conditional headers if caching is enabled
        FeedCacheEntry? cachedEntry = null;
        if (_options.EnableCaching)
        {
            cachedEntry = await _cache.GetAsync(uri.ToString(), cancellationToken);
            if (cachedEntry != null)
            {
                if (!string.IsNullOrEmpty(cachedEntry.ETag))
                {
                    request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(cachedEntry.ETag));
                }
                if (cachedEntry.LastModified.HasValue)
                {
                    request.Headers.IfModifiedSince = cachedEntry.LastModified.Value;
                }
            }
        }

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);

            var responseInfo = new HttpResponseInfo
            {
                StatusCode = response.StatusCode,
                FinalUrl = response.RequestMessage?.RequestUri,
                ETag = response.Headers.ETag?.Tag,
                LastModified = response.Content.Headers.LastModified,
                ContentType = response.Content.Headers.ContentType?.MediaType,
                ContentLength = response.Content.Headers.ContentLength,
                WasNotModified = response.StatusCode == HttpStatusCode.NotModified
            };

            // Handle 304 Not Modified
            if (response.StatusCode == HttpStatusCode.NotModified && cachedEntry != null)
            {
                _logger.LogDebug("Feed not modified, using cached version");
                return new FeedHttpResponse
                {
                    IsSuccess = true,
                    Content = null,
                    CachedFeed = cachedEntry.Feed,
                    ResponseInfo = responseInfo
                };
            }

            // Handle error responses
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Feed request failed with status {StatusCode}", response.StatusCode);
                return new FeedHttpResponse
                {
                    IsSuccess = false,
                    Error = CreateError(response.StatusCode),
                    ResponseInfo = responseInfo
                };
            }

            // Read content
            var content = await ReadContentAsync(response, cancellationToken);

            _logger.LogDebug("Successfully fetched feed ({ContentLength} bytes)", content.Length);

            return new FeedHttpResponse
            {
                IsSuccess = true,
                Content = content,
                ResponseInfo = responseInfo
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error fetching feed from {Url}", uri);
            return new FeedHttpResponse
            {
                IsSuccess = false,
                Error = FeedError.Network($"Network error: {ex.Message}", ex)
            };
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout fetching feed from {Url}", uri);
            return new FeedHttpResponse
            {
                IsSuccess = false,
                Error = FeedError.Timeout($"Request timed out", ex)
            };
        }
    }

    /// <summary>
    /// Caches a successfully parsed feed.
    /// </summary>
    public async Task CacheFeedAsync(
        string url,
        Feed feed,
        HttpResponseInfo? responseInfo,
        CancellationToken cancellationToken)
    {
        if (!_options.EnableCaching) return;

        var entry = new FeedCacheEntry
        {
            Feed = feed,
            ETag = responseInfo?.ETag,
            LastModified = responseInfo?.LastModified,
            CachedAt = DateTimeOffset.UtcNow
        };

        await _cache.SetAsync(url, entry, cancellationToken);
    }

    private void ApplyCredentials(HttpRequestMessage request, FeedCredentials? credentials)
    {
        credentials ??= _options.DefaultCredentials;
        if (credentials == null) return;

        switch (credentials.Type)
        {
            case AuthenticationType.Basic:
                if (!string.IsNullOrEmpty(credentials.Username))
                {
                    var encoded = Convert.ToBase64String(
                        Encoding.ASCII.GetBytes($"{credentials.Username}:{credentials.Password}"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encoded);
                }
                break;

            case AuthenticationType.Bearer:
                if (!string.IsNullOrEmpty(credentials.BearerToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", credentials.BearerToken);
                }
                break;

            case AuthenticationType.CustomHeaders:
                if (credentials.CustomHeaders != null)
                {
                    foreach (var header in credentials.CustomHeaders)
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
                break;

            case AuthenticationType.Cookie:
                if (credentials.Cookies != null)
                {
                    var cookieHeader = string.Join("; ",
                        credentials.Cookies.Select(c => $"{c.Key}={c.Value}"));
                    request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
                }
                break;
        }
    }

    private async Task<string> ReadContentAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        // Try to detect encoding from content-type header
        var encoding = GetEncodingFromContentType(response.Content.Headers.ContentType);

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        // Try to detect encoding from BOM or XML declaration
        encoding ??= DetectEncoding(bytes);

        return encoding.GetString(bytes);
    }

    private static Encoding? GetEncodingFromContentType(MediaTypeHeaderValue? contentType)
    {
        var charset = contentType?.CharSet;
        if (string.IsNullOrEmpty(charset)) return null;

        try
        {
            return Encoding.GetEncoding(charset);
        }
        catch
        {
            return null;
        }
    }

    private static Encoding DetectEncoding(byte[] bytes)
    {
        // Check for BOM
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return Encoding.UTF8;
        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            return Encoding.BigEndianUnicode;
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            return Encoding.Unicode;

        // Try to find encoding in XML declaration
        var preview = Encoding.ASCII.GetString(bytes, 0, Math.Min(bytes.Length, 200));
        var encodingMatch = System.Text.RegularExpressions.Regex.Match(
            preview, @"encoding\s*=\s*[""']([^""']+)[""']", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (encodingMatch.Success)
        {
            try
            {
                return Encoding.GetEncoding(encodingMatch.Groups[1].Value);
            }
            catch
            {
                // Fall through to default
            }
        }

        return Encoding.UTF8;
    }

    private static FeedError CreateError(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.NotFound => FeedError.NotFound("Feed not found (404)"),
            HttpStatusCode.Unauthorized => FeedError.Unauthorized("Authentication required (401)"),
            HttpStatusCode.Forbidden => FeedError.Unauthorized("Access forbidden (403)"),
            HttpStatusCode.RequestTimeout => FeedError.Timeout("Request timed out"),
            HttpStatusCode.GatewayTimeout => FeedError.Timeout("Gateway timeout"),
            _ => FeedError.Network($"HTTP error: {(int)statusCode} {statusCode}")
        };
    }
}

/// <summary>
/// Represents the response from fetching a feed.
/// </summary>
public sealed record FeedHttpResponse
{
    public bool IsSuccess { get; init; }
    public string? Content { get; init; }
    public Feed? CachedFeed { get; init; }
    public FeedError? Error { get; init; }
    public HttpResponseInfo? ResponseInfo { get; init; }
}
