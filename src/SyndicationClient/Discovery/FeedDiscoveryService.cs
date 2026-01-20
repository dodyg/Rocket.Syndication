using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SyndicationClient.Models;

namespace SyndicationClient.Discovery;

/// <summary>
/// Service for discovering feed URLs from HTML pages.
/// </summary>
public partial class FeedDiscoveryService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FeedDiscoveryService> _logger;

    public FeedDiscoveryService(HttpClient httpClient, ILogger<FeedDiscoveryService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Discovers feed URLs from an HTML page.
    /// </summary>
    public async Task<IReadOnlyList<FeedLink>> DiscoverAsync(string url, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Discovering feeds from {Url}", url);

        try
        {
            var baseUri = new Uri(url);
            var html = await _httpClient.GetStringAsync(baseUri, cancellationToken);
            var feeds = ParseFeedLinks(html, baseUri);

            _logger.LogDebug("Discovered {Count} feeds from {Url}", feeds.Count, url);
            return feeds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to discover feeds from {Url}", url);
            return [];
        }
    }

    private List<FeedLink> ParseFeedLinks(string html, Uri baseUri)
    {
        var feeds = new List<FeedLink>();

        var matches = LinkTagRegex().Matches(html);

        foreach (Match match in matches)
        {
            var attributes = match.Value;

            // Check if it's an alternate link
            if (!RelAlternateRegex().IsMatch(attributes))
                continue;

            // Get the type
            var typeMatch = TypeRegex().Match(attributes);
            if (!typeMatch.Success)
                continue;

            var type = typeMatch.Groups[1].Value.ToLowerInvariant();
            var feedType = type switch
            {
                "application/rss+xml" => FeedType.Rss,
                "application/atom+xml" => FeedType.Atom,
                "application/feed+json" => FeedType.Unknown, // JSON feed not supported yet
                _ => FeedType.Unknown
            };

            if (feedType == FeedType.Unknown)
                continue;

            // Get the href
            var hrefMatch = HrefRegex().Match(attributes);
            if (!hrefMatch.Success)
                continue;

            var href = System.Web.HttpUtility.HtmlDecode(hrefMatch.Groups[1].Value);
            if (!Uri.TryCreate(baseUri, href, out var feedUri))
                continue;

            // Get the title (optional)
            var titleMatch = TitleRegex().Match(attributes);
            var title = titleMatch.Success
                ? System.Web.HttpUtility.HtmlDecode(titleMatch.Groups[1].Value)
                : null;

            feeds.Add(new FeedLink
            {
                Url = feedUri,
                Title = title,
                Type = feedType
            });
        }

        return feeds;
    }

    [GeneratedRegex(@"<link\s+[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex LinkTagRegex();

    [GeneratedRegex(@"rel\s*=\s*[""']?alternate[""']?", RegexOptions.IgnoreCase)]
    private static partial Regex RelAlternateRegex();

    [GeneratedRegex(@"type\s*=\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase)]
    private static partial Regex TypeRegex();

    [GeneratedRegex(@"href\s*=\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase)]
    private static partial Regex HrefRegex();

    [GeneratedRegex(@"title\s*=\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase)]
    private static partial Regex TitleRegex();
}
