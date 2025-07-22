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
    public async Task<List<ResourceExample>> GetResourcesNormalAsync()
    {
        _logger.Log("Starting normal resource fetch...", LogLevelInternal.Information, "Normal Store");

        try
        {
            var resources = await _simpleStore.GetRandomResourcesAsync();
            _logger.Log($"Successfully fetched {resources.Count} resources normally", LogLevelInternal.Information, "Normal Store");
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
    public async Task<List<ResourceExample>> GetResourcesWithDeadlockRiskAsync()
    {
        _logger.Log("Starting deadlock-prone resource fetch...", LogLevelInternal.Warning, "Deadlock Store");

        try
        {
            var resources = await _deadlockStore.GetRandomResourcesAsync();
            _logger.Log($"Successfully fetched {resources.Count} resources (avoided deadlock)", LogLevelInternal.Information, "Deadlock Store");
            return resources;
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in deadlock-prone fetch: {ex.Message}", LogLevelInternal.Error, "Deadlock Store");
            throw;
        }
    }

    /// <summary>
    /// Demonstrates stampede scenario by running multiple concurrent requests.
    /// </summary>
    public async Task<List<List<ResourceExample>>> SimulateStampedeAsync(int numberOfTasks = 5, bool useDeadlockProne = false)
    {
        var storeName = useDeadlockProne ? "Deadlock Store" : "Normal Store";
        _logger.Log($"🚀 Starting stampede simulation with {numberOfTasks} concurrent tasks using {storeName}!", LogLevelInternal.Information, "Stampede Simulator");

        var tasks = new List<Task<List<ResourceExample>>>();

        for (int i = 0; i < numberOfTasks; i++)
        {
            var taskNumber = i + 1;
            _logger.Log($"Creating task {taskNumber}/{numberOfTasks}", LogLevelInternal.Debug, "Stampede Simulator");

            if (useDeadlockProne)
            {
                tasks.Add(GetResourcesWithDeadlockRiskAsync());
            }
            else
            {
                tasks.Add(GetResourcesNormalAsync());
            }
        }

        try
        {
            _logger.Log($"Waiting for all {numberOfTasks} tasks to complete...", LogLevelInternal.Information, "Stampede Simulator");
            var results = await Task.WhenAll(tasks);
            _logger.Log($"✅ Stampede simulation completed! All {numberOfTasks} tasks finished successfully.", LogLevelInternal.Information, "Stampede Simulator");
            return results.ToList();
        }
        catch (Exception ex)
        {
            _logger.Log($"💥 Stampede simulation failed: {ex.Message}", LogLevelInternal.Error, "Stampede Simulator");
            throw;
        }
    }

    /// <summary>
    /// Demonstrates resource fetching with caching.
    /// </summary>
    public async Task<List<ResourceExample>> GetResourcesCachedAsync()
    {
        _logger.Log("Starting cached resource fetch...", LogLevelInternal.Information, "Cached Store");

        try
        {
            var resources = await _cachedStore.GetRandomResourcesAsync();
            _logger.Log($"Successfully fetched {resources.Count} resources from cache", LogLevelInternal.Information, "Cached Store");
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
    public async Task<List<ResourceExample>> GetThreadSafeResourcesCachedAsync()
    {
        _logger.Log("Starting thread safe cached resource fetch...", LogLevelInternal.Information, "Thread Safe Cached Store");

        try
        {
            var resources = await _threadSafeCachedStore.GetRandomResourcesAsync();
            _logger.Log($"Successfully fetched {resources.Count} resources from cache", LogLevelInternal.Information, "Thread Safe Cached Store");
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
    /// Gets simple resources without delay for comparison.
    /// </summary>
    public List<ResourceExample> GetResourcesSimple()
    {
        _logger.Log("Getting resources with simple/fast method", LogLevelInternal.Information, "Simple Method");

        try
        {
            var resources = _simpleStore.GetRandomResourcesAsync().Result;
            _logger.Log($"Got {resources.Count} resources instantly", LogLevelInternal.Information, "Simple Method");
            return resources;
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in simple fetch: {ex.Message}", LogLevelInternal.Error, "Simple Method");
            throw;
        }
    }

    public void Dispose()
    {
        _cachedStore.Dispose();
        _threadSafeCachedStore.Dispose();
    }
}