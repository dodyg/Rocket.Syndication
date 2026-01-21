namespace Rocket.Syndication.Models;

/// <summary>
/// Represents a media enclosure attached to a feed item.
/// </summary>
public sealed record FeedEnclosure
{
    /// <summary>
    /// Gets the URL of the enclosure.
    /// </summary>
    public required Uri Url { get; init; }

    /// <summary>
    /// Gets the MIME type of the enclosure.
    /// </summary>
    public string? MimeType { get; init; }

    /// <summary>
    /// Gets the size of the enclosure in bytes.
    /// </summary>
    public long? Length { get; init; }
}
