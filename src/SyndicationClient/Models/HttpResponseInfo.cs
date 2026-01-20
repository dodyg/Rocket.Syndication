using System.Net;

namespace SyndicationClient.Models;

/// <summary>
/// Contains information about the HTTP response from a feed request.
/// </summary>
public sealed record HttpResponseInfo
{
    /// <summary>
    /// Gets the HTTP status code.
    /// </summary>
    public required HttpStatusCode StatusCode { get; init; }

    /// <summary>
    /// Gets the final URL after any redirects.
    /// </summary>
    public Uri? FinalUrl { get; init; }

    /// <summary>
    /// Gets the ETag header value.
    /// </summary>
    public string? ETag { get; init; }

    /// <summary>
    /// Gets the Last-Modified header value.
    /// </summary>
    public DateTimeOffset? LastModified { get; init; }

    /// <summary>
    /// Gets the Content-Type header value.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Gets the Content-Length header value.
    /// </summary>
    public long? ContentLength { get; init; }

    /// <summary>
    /// Gets whether the response was served from cache (304 Not Modified).
    /// </summary>
    public bool WasNotModified { get; init; }
}
