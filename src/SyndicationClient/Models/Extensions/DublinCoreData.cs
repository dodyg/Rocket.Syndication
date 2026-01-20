namespace SyndicationClient.Models.Extensions;

/// <summary>
/// Contains Dublin Core metadata extension data.
/// </summary>
public sealed record DublinCoreData
{
    /// <summary>
    /// Gets the creator (dc:creator).
    /// </summary>
    public string? Creator { get; init; }

    /// <summary>
    /// Gets the date (dc:date).
    /// </summary>
    public DateTimeOffset? Date { get; init; }

    /// <summary>
    /// Gets the subject (dc:subject).
    /// </summary>
    public string? Subject { get; init; }

    /// <summary>
    /// Gets the description (dc:description).
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the publisher (dc:publisher).
    /// </summary>
    public string? Publisher { get; init; }

    /// <summary>
    /// Gets the contributor (dc:contributor).
    /// </summary>
    public string? Contributor { get; init; }

    /// <summary>
    /// Gets the type (dc:type).
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// Gets the format (dc:format).
    /// </summary>
    public string? Format { get; init; }

    /// <summary>
    /// Gets the identifier (dc:identifier).
    /// </summary>
    public string? Identifier { get; init; }

    /// <summary>
    /// Gets the source (dc:source).
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Gets the language (dc:language).
    /// </summary>
    public string? Language { get; init; }

    /// <summary>
    /// Gets the relation (dc:relation).
    /// </summary>
    public string? Relation { get; init; }

    /// <summary>
    /// Gets the coverage (dc:coverage).
    /// </summary>
    public string? Coverage { get; init; }

    /// <summary>
    /// Gets the rights (dc:rights).
    /// </summary>
    public string? Rights { get; init; }
}
