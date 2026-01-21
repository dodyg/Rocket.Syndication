namespace Rocket.Syndication.Models;

/// <summary>
/// Represents the type of syndication feed.
/// </summary>
public enum FeedType
{
    /// <summary>
    /// Unknown or undetected feed type.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// RSS 2.0 feed format.
    /// </summary>
    Rss,

    /// <summary>
    /// Atom feed format.
    /// </summary>
    Atom
}
