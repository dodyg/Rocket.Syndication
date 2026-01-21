namespace Rocket.Syndication.Models.Rss;

/// <summary>
/// Contains RSS-specific feed data not captured in the unified model.
/// </summary>
public sealed record RssFeedData
{
    /// <summary>
    /// Gets the RSS version.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// Gets the managing editor email.
    /// </summary>
    public string? ManagingEditor { get; init; }

    /// <summary>
    /// Gets the webmaster email.
    /// </summary>
    public string? WebMaster { get; init; }

    /// <summary>
    /// Gets the documentation URL.
    /// </summary>
    public Uri? Docs { get; init; }

    /// <summary>
    /// Gets the cloud element for real-time updates.
    /// </summary>
    public RssCloud? Cloud { get; init; }

    /// <summary>
    /// Gets the time-to-live in minutes.
    /// </summary>
    public int? Ttl { get; init; }

    /// <summary>
    /// Gets the rating (PICS).
    /// </summary>
    public string? Rating { get; init; }

    /// <summary>
    /// Gets the text input element.
    /// </summary>
    public RssTextInput? TextInput { get; init; }

    /// <summary>
    /// Gets the hours to skip.
    /// </summary>
    public IReadOnlyList<int> SkipHours { get; init; } = [];

    /// <summary>
    /// Gets the days to skip.
    /// </summary>
    public IReadOnlyList<string> SkipDays { get; init; } = [];

    /// <summary>
    /// Gets the generator information.
    /// </summary>
    public string? Generator { get; init; }
}

/// <summary>
/// Represents an RSS cloud element for real-time updates.
/// </summary>
public sealed record RssCloud
{
    public string? Domain { get; init; }
    public int? Port { get; init; }
    public string? Path { get; init; }
    public string? RegisterProcedure { get; init; }
    public string? Protocol { get; init; }
}

/// <summary>
/// Represents an RSS text input element.
/// </summary>
public sealed record RssTextInput
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? Name { get; init; }
    public Uri? Link { get; init; }
}
