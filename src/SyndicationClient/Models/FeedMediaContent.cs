namespace SyndicationClient.Models;

/// <summary>
/// Represents media content associated with a feed item (Media RSS).
/// </summary>
public sealed record FeedMediaContent
{
    /// <summary>
    /// Gets the URL of the media content.
    /// </summary>
    public Uri? Url { get; init; }

    /// <summary>
    /// Gets the MIME type of the media.
    /// </summary>
    public string? MimeType { get; init; }

    /// <summary>
    /// Gets the medium type (image, video, audio, etc.).
    /// </summary>
    public string? Medium { get; init; }

    /// <summary>
    /// Gets the width of the media in pixels.
    /// </summary>
    public int? Width { get; init; }

    /// <summary>
    /// Gets the height of the media in pixels.
    /// </summary>
    public int? Height { get; init; }

    /// <summary>
    /// Gets the duration of the media in seconds.
    /// </summary>
    public int? Duration { get; init; }

    /// <summary>
    /// Gets the URL of the thumbnail image.
    /// </summary>
    public Uri? ThumbnailUrl { get; init; }

    /// <summary>
    /// Gets the title of the media.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets the description of the media.
    /// </summary>
    public string? Description { get; init; }
}
