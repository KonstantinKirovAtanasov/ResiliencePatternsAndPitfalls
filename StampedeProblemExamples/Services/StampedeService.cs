using StampedeProblem;
using StampedeProblem.Stores;

namespace StampedeProblemExamples.Services;

/// <summary>
/// Service that wraps stampede problem classes and logs their activities to the UI.
/// </summary>
public class StampedeService : IDisposable
{
    private readonly IRealTimeLogService _logger;
    private readonly SimpleResourceStore _simpleStore;
    private readonly DeadlockProneResourceStore _deadlockStore;
    private readonly ThreadSafeCachedResourceStore _threadSafeCachedStore;
    private readonly CachedResourceStore _cachedStore;

    public StampedeService(IRealTimeLogService logger)
    {
        _logger = logger;
        _simpleStore = new SimpleResourceStore(200, logger);
        _deadlockStore = new DeadlockProneResourceStore(300, logger);
        _cachedStore = new CachedResourceStore(200, logger);
        _threadSafeCachedStore = new ThreadSafeCachedResourceStore(200, logger);
    }

    /// <summary>
    /// Demonstrates normal resource fetching without deadlock issues.
    /// </summary>
    public async Task<(List<ResourceExample> result, long elapsedMilliseconds, long elapsedTicks)> GetResourcesNormalAsync()
    {
        _logger.Log("Starting normal resource fetch...", LogLevelInternal.Information, "Normal Store");

        try
        {
            var resources = await _simpleStore.GetRandomResourcesAsync();
            _logger.Log($"Successfully fetched {resources.result.Count} resources normally", LogLevelInternal.Information, "Normal Store");
            return resources;
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in normal fetch: {ex.Message}", LogLevelInternal.Error, "Normal Store");
            throw;
        }
    }

    /// <summary>
    /// Demonstrates resource fetching with deadlock vulnerability.
    /// </summary>
    public async Task<(List<ResourceExample> result, long elapsedMilliseconds, long elapsedTicks)> GetResourcesWithDeadlockRiskAsync()
    {
        _logger.Log("Starting deadlock-prone resource fetch...", LogLevelInternal.Warning, "Deadlock Store");

        try
        {
            var resources = await _deadlockStore.GetRandomResourcesAsync();
            _logger.Log($"Successfully fetched {resources.result.Count} resources (avoided deadlock)", LogLevelInternal.Information, "Deadlock Store");
            return resources;
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in deadlock-prone fetch: {ex.Message}", LogLevelInternal.Error, "Deadlock Store");
            throw;
        }
    }

    /// <summary>
    /// Demonstrates resource fetching with caching.
    /// </summary>
    public async Task<(List<ResourceExample> result, long elapsedMilliseconds, long elapsedTicks)> GetResourcesCachedAsync()
    {
        _logger.Log("Starting cached resource fetch...", LogLevelInternal.Information, "Cached Store");

        try
        {
            var resources = await _cachedStore.GetRandomResourcesAsync();
            _logger.Log($"Successfully fetched {resources.result.Count} resources from cache", LogLevelInternal.Information, "Cached Store");
            return resources;
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in cached fetch: {ex.Message}", LogLevelInternal.Error, "Cached Store");
            throw;
        }
    }

    /// <summary>
    /// Demonstrates resource fetching with tread safe caching to prevent stampede problems.
    /// </summary>
    public async Task<(List<ResourceExample> result, long elapsedMilliseconds, long elapsedTicks)> GetThreadSafeResourcesCachedAsync()
    {
        _logger.Log("Starting thread safe cached resource fetch...", LogLevelInternal.Information, "Thread Safe Cached Store");

        try
        {
            var resources = await _threadSafeCachedStore.GetRandomResourcesAsync();
            _logger.Log($"Successfully fetched {resources.result.Count} resources from cache", LogLevelInternal.Information, "Thread Safe Cached Store");
            return resources;
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in cached fetch: {ex.Message}", LogLevelInternal.Error, "Thread Safe Cached Store");
            throw;
        }
    }

    /// <summary>
    /// Clears the cache in the cached resource store.
    /// </summary>
    public void ClearCache()
    {
        _cachedStore.ClearCache();
        _logger.Log("Cache manually cleared", LogLevelInternal.Information, "Cache Manager");
    }

    /// <summary>
    /// Clears the cache in the cached resource store.
    /// </summary>
    public void ClearThreadSafeCache()
    {
        _threadSafeCachedStore.ClearCache();
        _logger.Log("Thread Safe Cache manually cleared", LogLevelInternal.Information, "Cache Manager");
    }
    
    /// <summary>
    /// Disposes the service and its resources, including cached stores.
    /// </summary>
    public void Dispose()
    {
        _cachedStore.Dispose();
        _threadSafeCachedStore.Dispose();
    }
}