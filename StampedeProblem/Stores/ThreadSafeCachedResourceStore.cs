using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace StampedeProblem.Stores;

/// <summary>
/// Represents a cached resource store that uses IMemoryCache to cache resources with a 10-minute expiration.
/// </summary>
public class ThreadSafeCachedResourceStore : SimpleResourceStore, IDisposable
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);
    private const string CACE_KEY = "RandomResources";
    private ConcurrentDictionary<string, SemaphoreSlim> _semaphoreDictionary = [];

    public ThreadSafeCachedResourceStore(int delayMs = 200, IRealTimeLogService? logger = null)
        : base(delayMs, logger)
        => _cache = new MemoryCache(new MemoryCacheOptions());

    /// <summary>
    /// Gets 20 random resources from cache or loads them if not cached, with 10-minute expiration.
    /// </summary>
    /// <returns>A collection of cached or freshly loaded random resources.</returns>
    public override async Task<(List<ResourceExample> result, long elapsedMilliseconds, long elapsedTicks)> GetRandomResourcesAsync()
    {
        var threadId = Thread.CurrentThread.ManagedThreadId;

        Stopwatch stopwatch = Stopwatch.StartNew();
        if (!_cache.TryGetValue(CACE_KEY, out List<ResourceExample>? cachedResources) || cachedResources == null)
        {
            // Най-важната разлика
            try
            {
                SemaphoreSlim semaphore = _semaphoreDictionary.GetOrAdd(CACE_KEY, _ => new(1, 1));
                var cacheMissMessage = $"Thread {threadId}: Cache MISS check waiting to release.";
                _logger?.Log(cacheMissMessage, LogLevelInternal.Information, "ThreadSafeCachedResourceStore");
                await semaphore.WaitAsync();
                if (_cache.TryGetValue(CACE_KEY, out var existingValue))
                {
                    cacheMissMessage = $"Thread {threadId}: Store data already loaded and populated.";
                    _logger?.Log(cacheMissMessage, LogLevelInternal.Information, "ThreadSafeCachedResourceStore");
                    return ((List<ResourceExample>?)existingValue!, stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks);
                }

                cacheMissMessage = $"Thread {threadId}: Cache MISS Step forward - Loading resources from store";
                _logger?.Log(cacheMissMessage, LogLevelInternal.Information, "ThreadSafeCachedResourceStore", true);

                var resources = await base.GetRandomResourcesAsync();
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _cacheExpiration,
                    Priority = CacheItemPriority.Normal
                };

                _cache.Set(CACE_KEY, resources.result, cacheOptions);
                cachedResources = resources.result;
            }
            finally
            {
                if (_semaphoreDictionary.TryGetValue(CACE_KEY, out var semaphore)) semaphore.Release();
            }

            var cachedMessage = $"Thread {threadId}: Resources cached for {_cacheExpiration.TotalMinutes} minutes";
            _logger?.Log(cachedMessage, LogLevelInternal.Information, "ThreadSafeCachedResourceStore");
        }

        var cacheHitMessage = $"Thread {threadId}: Cache HIT - Returning cached resources";
        _logger?.Log(cacheHitMessage, LogLevelInternal.Information, "ThreadSafeCachedResourceStore");

        cachedResources.ForEach(p => _logger?.Log(p.ToString(), LogLevelInternal.Debug, nameof(CachedResourceStore)));
        return (cachedResources, stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks);
    }

    /// <summary>
    /// Clears the cached resources.
    /// </summary>
    public void ClearCache()
    {
        //Също важно
        _cache.Remove(CACE_KEY);
        var message = "Cache cleared";
        _logger?.Log(message, LogLevelInternal.Information, "ThreadSafeCachedResourceStore");

        foreach (var semaphore in _semaphoreDictionary.Values) semaphore.Dispose();
        message = "Semapgors disposed";
        _logger?.Log(message, LogLevelInternal.Debug, "ThreadSafeCachedResourceStore");

        _semaphoreDictionary.Clear();
        message = "Semapgors cleared";
        _logger?.Log(message, LogLevelInternal.Debug, "ThreadSafeCachedResourceStore");
    }

    /// <summary>
    /// Disposes the cache if it was created by this instance.
    /// </summary>
    public void Dispose()
    {
        //Също важно
        _cache.Remove(CACE_KEY);
        foreach (var semaphore in _semaphoreDictionary.Values) semaphore.Dispose();
        _semaphoreDictionary.Clear();
        _cache.Dispose();
    }
}
