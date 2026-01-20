using SyndicationClient.Models.Atom;
using SyndicationClient.Models.Extensions;
using SyndicationClient.Models.Rss;

namespace SyndicationClient.Models;

/// <summary>
/// Represents a unified feed item that normalizes RSS items and Atom entries.
/// </summary>
public sealed record FeedItem
{
    /// <summary>
    /// Gets the unique identifier of the item.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the title of the item.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets the primary link to the item.
    /// </summary>
    public Uri? Link { get; init; }

    /// <summary>
    /// Gets the content of the item.
    /// </summary>
    public FeedContent? Content { get; init; }

    /// <summary>
    /// Gets the publication date.
    /// </summary>
    public DateTimeOffset? PublishedDate { get; init; }

    /// <summary>
    /// Gets the last updated date.
    /// </summary>
    public DateTimeOffset? UpdatedDate { get; init; }

    /// <summary>
    /// Gets the authors of the item.
    /// </summary>
    public IReadOnlyList<FeedAuthor> Authors { get; init; } = [];

    /// <summary>
    /// Gets the categories of the item.
    /// </summary>
    public IReadOnlyList<FeedCategory> Categories { get; init; } = [];

    /// <summary>
    /// Gets the enclosures (attachments) of the item.
    /// </summary>
    public IReadOnlyList<FeedEnclosure> Enclosures { get; init; } = [];

    /// <summary>
    /// Gets the media content of the item.
    /// </summary>
    public FeedMediaContent? Media { get; init; }

    /// <summary>
    /// Gets the RSS-specific data, if this was parsed from an RSS feed.
    /// </summary>
    public RssItemData? RssData { get; init; }

    /// <summary>
    /// Gets the Atom-specific data, if this was parsed from an Atom feed.
    /// </summary>
    public AtomEntryData? AtomData { get; init; }

    /// <summary>
    /// Gets the Dublin Core extension data.
    /// </summary>
    public DublinCoreData? DublinCore { get; init; }

    /// <summary>
    /// Gets the iTunes podcast extension data.
    /// </summary>
    public ITunesData? ITunes { get; init; }
}
