namespace Rocket.Syndication.Models;

/// <summary>
/// Represents the content of a feed item with both HTML and plain text versions.
/// </summary>
public sealed record FeedContent
{
    /// <summary>
    /// Gets the raw HTML content.
    /// </summary>
    public string? Html { get; init; }

    /// <summary>
    /// Gets the plain text version with HTML tags stripped.
    /// </summary>
    public string? PlainText { get; init; }

    /// <summary>
    /// Gets the MIME type of the content.
    /// </summary>
    public string? ContentType { get; init; }
}
