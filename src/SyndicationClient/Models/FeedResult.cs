namespace SyndicationClient.Models;

/// <summary>
/// Represents the result of a feed operation.
/// </summary>
public sealed record FeedResult
{
    /// <summary>
    /// Gets whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the parsed feed, if successful.
    /// </summary>
    public Feed? Feed { get; init; }

    /// <summary>
    /// Gets the error, if the operation failed.
    /// </summary>
    public FeedError? Error { get; init; }

    /// <summary>
    /// Gets information about the HTTP response.
    /// </summary>
    public HttpResponseInfo? ResponseInfo { get; init; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static FeedResult Success(Feed feed, HttpResponseInfo? responseInfo = null) =>
        new() { IsSuccess = true, Feed = feed, ResponseInfo = responseInfo };

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static FeedResult Failure(FeedError error, HttpResponseInfo? responseInfo = null) =>
        new() { IsSuccess = false, Error = error, ResponseInfo = responseInfo };

    /// <summary>
    /// Creates a result indicating the content was not modified (304).
    /// </summary>
    public static FeedResult NotModified(Feed? cachedFeed, HttpResponseInfo responseInfo) =>
        new() { IsSuccess = true, Feed = cachedFeed, ResponseInfo = responseInfo };
}
