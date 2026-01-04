namespace SyndicationClient.Models;

/// <summary>
/// Represents an author of a feed or feed item.
/// </summary>
public sealed record FeedAuthor
{
    /// <summary>
    /// Gets the name of the author.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets the email address of the author.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Gets the URI associated with the author.
    /// </summary>
    public Uri? Uri { get; init; }
}
