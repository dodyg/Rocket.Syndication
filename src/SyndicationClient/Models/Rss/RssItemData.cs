namespace SyndicationClient.Models.Rss;

/// <summary>
/// Contains RSS-specific item data not captured in the unified model.
/// </summary>
public sealed record RssItemData
{
    /// <summary>
    /// Gets the GUID (globally unique identifier).
    /// </summary>
    public string? Guid { get; init; }

    /// <summary>
    /// Gets whether the GUID is a permalink.
    /// </summary>
    public bool? IsPermaLink { get; init; }

    /// <summary>
    /// Gets the comments URL.
    /// </summary>
    public Uri? Comments { get; init; }

    /// <summary>
    /// Gets the source feed information.
    /// </summary>
    public RssSource? Source { get; init; }
}

/// <summary>
/// Represents the source of an RSS item.
/// </summary>
public sealed record RssSource
{
    /// <summary>
    /// Gets the name of the source.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets the URL of the source feed.
    /// </summary>
    public Uri? Url { get; init; }
}
