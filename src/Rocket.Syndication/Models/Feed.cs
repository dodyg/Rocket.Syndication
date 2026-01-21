using Rocket.Syndication.Models.Atom;
using Rocket.Syndication.Models.Extensions;
using Rocket.Syndication.Models.Rss;

namespace Rocket.Syndication.Models;

/// <summary>
/// Represents a unified syndication feed that normalizes RSS and Atom feeds.
/// </summary>
public sealed record Feed
{
    /// <summary>
    /// Gets the title of the feed.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the description or subtitle of the feed.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the primary link to the feed's website.
    /// </summary>
    public Uri? Link { get; init; }

    /// <summary>
    /// Gets the last updated date of the feed.
    /// </summary>
    public DateTimeOffset? LastUpdated { get; init; }

    /// <summary>
    /// Gets the language of the feed.
    /// </summary>
    public string? Language { get; init; }

    /// <summary>
    /// Gets the copyright information.
    /// </summary>
    public string? Copyright { get; init; }

    /// <summary>
    /// Gets the image associated with the feed.
    /// </summary>
    public FeedImage? Image { get; init; }

    /// <summary>
    /// Gets the items in the feed.
    /// </summary>
    public IReadOnlyList<FeedItem> Items { get; init; } = [];

    /// <summary>
    /// Gets the categories of the feed.
    /// </summary>
    public IReadOnlyList<FeedCategory> Categories { get; init; } = [];

    /// <summary>
    /// Gets the authors of the feed.
    /// </summary>
    public IReadOnlyList<FeedAuthor> Authors { get; init; } = [];

    /// <summary>
    /// Gets the type of feed (RSS or Atom).
    /// </summary>
    public FeedType Type { get; init; }

    /// <summary>
    /// Gets the RSS-specific data, if this was parsed from an RSS feed.
    /// </summary>
    public RssFeedData? RssData { get; init; }

    /// <summary>
    /// Gets the Atom-specific data, if this was parsed from an Atom feed.
    /// </summary>
    public AtomFeedData? AtomData { get; init; }

    /// <summary>
    /// Gets the Dublin Core extension data.
    /// </summary>
    public DublinCoreData? DublinCore { get; init; }

    /// <summary>
    /// Gets the iTunes podcast extension data.
    /// </summary>
    public ITunesData? ITunes { get; init; }
}
