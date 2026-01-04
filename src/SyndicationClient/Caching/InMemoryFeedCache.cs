using System.Collections.Concurrent;

namespace SyndicationClient.Caching;

/// <summary>
/// In-memory implementation of the feed cache.
/// </summary>
public class InMemoryFeedCache : IFeedCache
{
    private readonly ConcurrentDictionary<string, FeedCacheEntry> _cache = new();

    /// <inheritdoc />
    public Task<FeedCacheEntry?> GetAsync(string url, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _cache.TryGetValue(NormalizeUrl(url), out var entry);
        return Task.FromResult(entry);
    }

    /// <inheritdoc />
    public Task SetAsync(string url, FeedCacheEntry entry, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _cache[NormalizeUrl(url)] = entry;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveAsync(string url, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _cache.TryRemove(NormalizeUrl(url), out _);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ClearAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _cache.Clear();
        return Task.CompletedTask;
    }

    private static string NormalizeUrl(string url)
    {
        return url.TrimEnd('/').ToLowerInvariant();
    }
}
