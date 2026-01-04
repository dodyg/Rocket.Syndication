namespace SyndicationClient.Models;

/// <summary>
/// Represents an error that occurred during feed processing.
/// </summary>
public sealed record FeedError
{
    /// <summary>
    /// Gets the type of error.
    /// </summary>
    public required FeedErrorType Type { get; init; }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets the inner exception that caused this error, if any.
    /// </summary>
    public Exception? InnerException { get; init; }

    /// <summary>
    /// Creates a network error.
    /// </summary>
    public static FeedError Network(string message, Exception? innerException = null) =>
        new() { Type = FeedErrorType.NetworkError, Message = message, InnerException = innerException };

    /// <summary>
    /// Creates a timeout error.
    /// </summary>
    public static FeedError Timeout(string message, Exception? innerException = null) =>
        new() { Type = FeedErrorType.Timeout, Message = message, InnerException = innerException };

    /// <summary>
    /// Creates a not found error.
    /// </summary>
    public static FeedError NotFound(string message) =>
        new() { Type = FeedErrorType.NotFound, Message = message };

    /// <summary>
    /// Creates an unauthorized error.
    /// </summary>
    public static FeedError Unauthorized(string message) =>
        new() { Type = FeedErrorType.Unauthorized, Message = message };

    /// <summary>
    /// Creates a parse error.
    /// </summary>
    public static FeedError Parse(string message, Exception? innerException = null) =>
        new() { Type = FeedErrorType.ParseError, Message = message, InnerException = innerException };

    /// <summary>
    /// Creates an invalid feed error.
    /// </summary>
    public static FeedError InvalidFeed(string message) =>
        new() { Type = FeedErrorType.InvalidFeed, Message = message };

    /// <summary>
    /// Creates an encoding error.
    /// </summary>
    public static FeedError Encoding(string message, Exception? innerException = null) =>
        new() { Type = FeedErrorType.EncodingError, Message = message, InnerException = innerException };
}
