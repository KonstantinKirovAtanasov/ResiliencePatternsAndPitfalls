using Microsoft.Extensions.Caching.Memory;

namespace StampedeProblem.Stores;

/// <summary>
/// Represents a cached resource store that uses IMemoryCache to cache resources with a 10-minute expiration.
/// </summary>
public class CachedResourceStore : SimpleResourceStore, IDisposable
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);
    private const string CACE_KEY = "RandomResources";

    public CachedResourceStore(int delayMs = 200, IRealTimeLogService? logger = null)
        : base(delayMs, logger)
        => _cache = new MemoryCache(new MemoryCacheOptions());


    /// <summary>
    /// Gets 20 random resources from cache or loads them if not cached, with 10-minute expiration.
    /// </summary>
    /// <returns>A collection of cached or freshly loaded random resources.</returns>
    public override async Task<List<ResourceExample>> GetRandomResourcesAsync()
    {
        var threadId = Thread.CurrentThread.ManagedThreadId;
        if (!_cache.TryGetValue(CACE_KEY, out List<ResourceExample>? cachedResources) || cachedResources == null)
        {
            var cacheMissMessage = $"Thread {threadId}: Cache MISS - Loading resources from store";
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {cacheMissMessage}");
            _logger?.Log(cacheMissMessage, LogLevelInternal.Information, "CachedResourceStore", true);

            var resources = await base.GetRandomResourcesAsync();
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheExpiration,
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(CACE_KEY, resources, cacheOptions);

            cachedResources = resources;
            var cachedMessage = $"Thread {threadId}: Resources cached for {_cacheExpiration.TotalMinutes} minutes";
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {cachedMessage}");
            _logger?.Log(cachedMessage, LogLevelInternal.Information, "CachedResourceStore");
        }

        var cacheHitMessage = $"Thread {threadId}: Cache HIT - Returning cached resources";
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {cacheHitMessage}");
        _logger?.Log(cacheHitMessage, LogLevelInternal.Information, "CachedResourceStore");

        cachedResources.ForEach(p => _logger?.Log(p.ToString(), LogLevelInternal.Debug, nameof(CachedResourceStore)));
        return cachedResources;
    }

    /// <summary>
    /// Clears the cached resources.
    /// </summary>
    public void ClearCache()
    {
        _cache.Remove(CACE_KEY);
        var message = "Cache cleared";
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
        _logger?.Log(message, LogLevelInternal.Information, "CachedResourceStore");
    }

    /// <summary>
    /// Disposes the cache if it was created by this instance.
    /// </summary>
    public void Dispose() => _cache.Dispose();
}
