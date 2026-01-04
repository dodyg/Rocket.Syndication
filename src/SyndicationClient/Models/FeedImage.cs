namespace SyndicationClient.Models;

/// <summary>
/// Represents an image associated with a feed.
/// </summary>
public sealed record FeedImage
{
    /// <summary>
    /// Gets the URL of the image.
    /// </summary>
    public required Uri Url { get; init; }

    /// <summary>
    /// Gets the title of the image.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets the link associated with the image.
    /// </summary>
    public Uri? Link { get; init; }

    /// <summary>
    /// Gets the width of the image in pixels.
    /// </summary>
    public int? Width { get; init; }

    /// <summary>
    /// Gets the height of the image in pixels.
    /// </summary>
    public int? Height { get; init; }

    /// <summary>
    /// Gets the description of the image.
    /// </summary>
    public string? Description { get; init; }
}
