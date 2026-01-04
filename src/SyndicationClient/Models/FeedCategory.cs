namespace SyndicationClient.Models;

/// <summary>
/// Represents a category or tag associated with a feed or feed item.
/// </summary>
public sealed record FeedCategory
{
    /// <summary>
    /// Gets the name or term of the category.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the domain or scheme of the category.
    /// </summary>
    public string? Domain { get; init; }

    /// <summary>
    /// Gets the human-readable label for the category.
    /// </summary>
    public string? Label { get; init; }
}
