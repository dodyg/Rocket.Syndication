using SyndicationClient.Authentication;
using SyndicationClient.Models;

namespace SyndicationClient;

/// <summary>
/// Defines the contract for a syndication feed client.
/// </summary>
public interface ISyndicationClient
{
    /// <summary>
    /// Fetches and parses a feed from the specified URL.
    /// </summary>
    /// <param name="url">The URL of the feed.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the feed operation.</returns>
    Task<FeedResult> GetFeedAsync(string url, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches and parses a feed from the specified URI.
    /// </summary>
    /// <param name="uri">The URI of the feed.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the feed operation.</returns>
    Task<FeedResult> GetFeedAsync(Uri uri, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches and parses a feed from the specified URL with credentials.
    /// </summary>
    /// <param name="url">The URL of the feed.</param>
    /// <param name="credentials">The credentials to use for authentication.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the feed operation.</returns>
    Task<FeedResult> GetFeedAsync(string url, FeedCredentials credentials, CancellationToken cancellationToken);

    /// <summary>
    /// Parses a feed from the provided string content.
    /// </summary>
    /// <param name="content">The feed content as a string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the feed operation.</returns>
    Task<FeedResult> ParseFeedAsync(string content, CancellationToken cancellationToken);

    /// <summary>
    /// Parses a feed from the provided stream.
    /// </summary>
    /// <param name="stream">The stream containing the feed content.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the feed operation.</returns>
    Task<FeedResult> ParseFeedAsync(Stream stream, CancellationToken cancellationToken);

    /// <summary>
    /// Discovers feed URLs from an HTML page.
    /// </summary>
    /// <param name="htmlUrl">The URL of the HTML page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of discovered feed links.</returns>
    Task<IReadOnlyList<FeedLink>> DiscoverFeedsAsync(string htmlUrl, CancellationToken cancellationToken);
}
