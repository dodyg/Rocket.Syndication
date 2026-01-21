using Microsoft.Extensions.Logging;
using Rocket.Syndication.Authentication;
using Rocket.Syndication.Discovery;
using Rocket.Syndication.Http;
using Rocket.Syndication.Models;
using Rocket.Syndication.Parsing;

namespace Rocket.Syndication;

/// <summary>
/// Default implementation of the syndication client.
/// </summary>
public class SyndicationFeedClient : ISyndicationClient
{
    private readonly FeedHttpClient _httpClient;
    private readonly FeedParserPipeline _parserPipeline;
    private readonly FeedDiscoveryService _discoveryService;
    private readonly ILogger<SyndicationFeedClient> _logger;

    public SyndicationFeedClient(
        FeedHttpClient httpClient,
        FeedParserPipeline parserPipeline,
        FeedDiscoveryService discoveryService,
        ILogger<SyndicationFeedClient> logger)
    {
        _httpClient = httpClient;
        _parserPipeline = parserPipeline;
        _discoveryService = discoveryService;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<FeedResult> GetFeedAsync(string url, CancellationToken cancellationToken)
    {
        return GetFeedAsync(url, null, cancellationToken);
    }

    /// <inheritdoc />
    public Task<FeedResult> GetFeedAsync(Uri uri, CancellationToken cancellationToken)
    {
        return GetFeedAsync(uri.ToString(), null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FeedResult> GetFeedAsync(string url, FeedCredentials? credentials, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting feed from {Url}", url);

        var response = await _httpClient.GetAsync(url, credentials, cancellationToken);

        if (!response.IsSuccess)
        {
            return FeedResult.Failure(response.Error!, response.ResponseInfo);
        }

        // Return cached feed if 304 Not Modified
        if (response.CachedFeed != null)
        {
            return FeedResult.NotModified(response.CachedFeed, response.ResponseInfo!);
        }

        // Parse the feed
        var result = _parserPipeline.Parse(response.Content!);

        if (result.IsSuccess && result.Feed != null)
        {
            // Cache the parsed feed
            await _httpClient.CacheFeedAsync(url, result.Feed, response.ResponseInfo, cancellationToken);

            return FeedResult.Success(result.Feed, response.ResponseInfo);
        }

        return FeedResult.Failure(result.Error!, response.ResponseInfo);
    }

    /// <inheritdoc />
    public Task<FeedResult> ParseFeedAsync(string content, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogDebug("Parsing feed from string content");
        var result = _parserPipeline.Parse(content);

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public async Task<FeedResult> ParseFeedAsync(Stream stream, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Parsing feed from stream");
        return await _parserPipeline.ParseAsync(stream, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<FeedLink>> DiscoverFeedsAsync(string htmlUrl, CancellationToken cancellationToken)
    {
        return _discoveryService.DiscoverAsync(htmlUrl, cancellationToken);
    }
}
