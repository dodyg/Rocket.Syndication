namespace SyndicationClient.Models.Extensions;

/// <summary>
/// Contains iTunes podcast extension data.
/// </summary>
public sealed record ITunesData
{
    /// <summary>
    /// Gets the author.
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// Gets the subtitle.
    /// </summary>
    public string? Subtitle { get; init; }

    /// <summary>
    /// Gets the summary.
    /// </summary>
    public string? Summary { get; init; }

    /// <summary>
    /// Gets the image URL.
    /// </summary>
    public Uri? ImageUrl { get; init; }

    /// <summary>
    /// Gets the duration in seconds.
    /// </summary>
    public int? Duration { get; init; }

    /// <summary>
    /// Gets whether this is explicit content.
    /// </summary>
    public bool? Explicit { get; init; }

    /// <summary>
    /// Gets the episode number.
    /// </summary>
    public int? Episode { get; init; }

    /// <summary>
    /// Gets the season number.
    /// </summary>
    public int? Season { get; init; }

    /// <summary>
    /// Gets the episode type.
    /// </summary>
    public string? EpisodeType { get; init; }

    /// <summary>
    /// Gets the owner information.
    /// </summary>
    public ITunesOwner? Owner { get; init; }

    /// <summary>
    /// Gets the categories.
    /// </summary>
    public IReadOnlyList<ITunesCategory> Categories { get; init; } = [];

    /// <summary>
    /// Gets the keywords.
    /// </summary>
    public IReadOnlyList<string> Keywords { get; init; } = [];

    /// <summary>
    /// Gets whether the feed is blocked.
    /// </summary>
    public bool? Block { get; init; }

    /// <summary>
    /// Gets whether the feed is complete.
    /// </summary>
    public bool? Complete { get; init; }
}

/// <summary>
/// Represents an iTunes owner.
/// </summary>
public sealed record ITunesOwner
{
    public string? Name { get; init; }
    public string? Email { get; init; }
}

/// <summary>
/// Represents an iTunes category.
/// </summary>
public sealed record ITunesCategory
{
    public required string Text { get; init; }
    public ITunesCategory? Subcategory { get; init; }
}
