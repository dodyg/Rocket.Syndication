using Rocket.Syndication.Models;

namespace Rocket.Syndication.Caching;

/// <summary>
/// Defines the contract for a feed cache.
/// </summary>
public interface IFeedCache
{
    /// <summary>
    /// Gets cached data for the specified URL.
    /// </summary>
    /// <param name="url">The feed URL.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The cached entry if available; otherwise, null.</returns>
    Task<FeedCacheEntry?> GetAsync(string url, CancellationToken cancellationToken);

    /// <summary>
    /// Stores feed data in the cache.
    /// </summary>
    /// <param name="url">The feed URL.</param>
    /// <param name="entry">The cache entry to store.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SetAsync(string url, FeedCacheEntry entry, CancellationToken cancellationToken);

    /// <summary>
    /// Removes cached data for the specified URL.
    /// </summary>
    /// <param name="url">The feed URL.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RemoveAsync(string url, CancellationToken cancellationToken);

    /// <summary>
    /// Clears all cached data.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ClearAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Represents a cached feed entry.
/// </summary>
public sealed record FeedCacheEntry
{
    /// <summary>
    /// Gets the cached feed.
    /// </summary>
    public required Feed Feed { get; init; }

    /// <summary>
    /// Gets the ETag from the last response.
    /// </summary>
    public string? ETag { get; init; }

    /// <summary>
    /// Gets the Last-Modified date from the last response.
    /// </summary>
    public DateTimeOffset? LastModified { get; init; }

    /// <summary>
    /// Gets when this entry was cached.
    /// </summary>
    public DateTimeOffset CachedAt { get; init; }
}
