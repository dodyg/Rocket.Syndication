namespace SyndicationClient.Models.Atom;

/// <summary>
/// Contains Atom-specific feed data not captured in the unified model.
/// </summary>
public sealed record AtomFeedData
{
    /// <summary>
    /// Gets the feed ID (required in Atom).
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Gets the generator information.
    /// </summary>
    public AtomGenerator? Generator { get; init; }

    /// <summary>
    /// Gets the icon URL.
    /// </summary>
    public Uri? Icon { get; init; }

    /// <summary>
    /// Gets the logo URL.
    /// </summary>
    public Uri? Logo { get; init; }

    /// <summary>
    /// Gets the subtitle.
    /// </summary>
    public string? Subtitle { get; init; }

    /// <summary>
    /// Gets all links associated with the feed.
    /// </summary>
    public IReadOnlyList<AtomLink> Links { get; init; } = [];

    /// <summary>
    /// Gets the contributors.
    /// </summary>
    public IReadOnlyList<AtomPerson> Contributors { get; init; } = [];
}

/// <summary>
/// Represents an Atom generator element.
/// </summary>
public sealed record AtomGenerator
{
    public string? Name { get; init; }
    public Uri? Uri { get; init; }
    public string? Version { get; init; }
}

/// <summary>
/// Represents an Atom link element.
/// </summary>
public sealed record AtomLink
{
    public required Uri Href { get; init; }
    public string? Rel { get; init; }
    public string? Type { get; init; }
    public string? HrefLang { get; init; }
    public string? Title { get; init; }
    public long? Length { get; init; }
}

/// <summary>
/// Represents an Atom person construct.
/// </summary>
public sealed record AtomPerson
{
    public string? Name { get; init; }
    public string? Email { get; init; }
    public Uri? Uri { get; init; }
}
