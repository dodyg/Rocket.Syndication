namespace Rocket.Syndication.Models.Atom;

/// <summary>
/// Contains Atom-specific entry data not captured in the unified model.
/// </summary>
public sealed record AtomEntryData
{
    /// <summary>
    /// Gets the entry ID (required in Atom).
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Gets the summary.
    /// </summary>
    public AtomText? Summary { get; init; }

    /// <summary>
    /// Gets the content element.
    /// </summary>
    public AtomContent? Content { get; init; }

    /// <summary>
    /// Gets all links associated with the entry.
    /// </summary>
    public IReadOnlyList<AtomLink> Links { get; init; } = [];

    /// <summary>
    /// Gets the contributors.
    /// </summary>
    public IReadOnlyList<AtomPerson> Contributors { get; init; } = [];

    /// <summary>
    /// Gets the source feed information.
    /// </summary>
    public AtomSource? Source { get; init; }

    /// <summary>
    /// Gets the rights information.
    /// </summary>
    public string? Rights { get; init; }
}

/// <summary>
/// Represents an Atom text construct.
/// </summary>
public sealed record AtomText
{
    public string? Value { get; init; }
    public string? Type { get; init; }
}

/// <summary>
/// Represents an Atom content element.
/// </summary>
public sealed record AtomContent
{
    public string? Value { get; init; }
    public string? Type { get; init; }
    public Uri? Src { get; init; }
}

/// <summary>
/// Represents an Atom source element.
/// </summary>
public sealed record AtomSource
{
    public string? Id { get; init; }
    public string? Title { get; init; }
    public DateTimeOffset? Updated { get; init; }
    public IReadOnlyList<AtomLink> Links { get; init; } = [];
}
