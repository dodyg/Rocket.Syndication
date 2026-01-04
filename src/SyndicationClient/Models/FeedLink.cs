namespace SyndicationClient.Models;

/// <summary>
/// Represents a discovered feed link from an HTML page.
/// </summary>
public sealed record FeedLink
{
    /// <summary>
    /// Gets the URL of the feed.
    /// </summary>
    public required Uri Url { get; init; }

    /// <summary>
    /// Gets the title of the feed.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets the type of the feed.
    /// </summary>
    public FeedType Type { get; init; }
}
