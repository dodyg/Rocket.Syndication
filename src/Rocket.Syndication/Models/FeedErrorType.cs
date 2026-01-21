namespace Rocket.Syndication.Models;

/// <summary>
/// Represents the type of error that occurred during feed processing.
/// </summary>
public enum FeedErrorType
{
    /// <summary>
    /// A network-related error occurred.
    /// </summary>
    NetworkError,

    /// <summary>
    /// The request timed out.
    /// </summary>
    Timeout,

    /// <summary>
    /// The feed was not found (404).
    /// </summary>
    NotFound,

    /// <summary>
    /// Authentication failed (401/403).
    /// </summary>
    Unauthorized,

    /// <summary>
    /// Failed to parse the feed content.
    /// </summary>
    ParseError,

    /// <summary>
    /// The feed content is invalid or malformed.
    /// </summary>
    InvalidFeed,

    /// <summary>
    /// Character encoding error occurred.
    /// </summary>
    EncodingError
}
