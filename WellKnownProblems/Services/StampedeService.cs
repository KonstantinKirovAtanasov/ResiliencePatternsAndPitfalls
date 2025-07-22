using StampedeProblem;
using WellKnownProblems.Services;

namespace WellKnownProblems.Services;

/// <summary>
/// Service that wraps stampede problem classes and logs their activities to the UI.
/// </summary>
public class StampedeService
{
    private readonly RealTimeLogService _logger;
    private readonly SimpleResourceStore _simpleStore;
    private readonly DeadlockProneResourceStore _deadlockStore;

    public StampedeService(RealTimeLogService logger)
    {
        _logger = logger;
        _simpleStore = new SimpleResourceStore(200);
        _deadlockStore = new DeadlockProneResourceStore(300);
    }

    /// <summary>
    /// Demonstrates normal resource fetching without deadlock issues.
    /// </summary>
    public async Task<List<ResourceExample>> GetResourcesNormalAsync()
    {
        _logger.Log("Starting normal resource fetch...", LogLevel.Information, "Normal Store");

        try
        {
            var resources = await _simpleStore.GetRandomResourcesAsync();
            _logger.Log($"Successfully fetched {resources.Count} resources normally", LogLevel.Information, "Normal Store");
            return resources;
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in normal fetch: {ex.Message}", LogLevel.Error, "Normal Store");
            throw;
        }
    }

    /// <summary>
    /// Demonstrates resource fetching with deadlock vulnerability.
    /// </summary>
    public async Task<List<ResourceExample>> GetResourcesWithDeadlockRiskAsync()
    {
        _logger.Log("Starting deadlock-prone resource fetch...", LogLevel.Warning, "Deadlock Store");

        try
        {
            var resources = await _deadlockStore.GetRandomResourcesAsync();
            _logger.Log($"Successfully fetched {resources.Count} resources (avoided deadlock)", LogLevel.Information, "Deadlock Store");
            return resources;
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in deadlock-prone fetch: {ex.Message}", LogLevel.Error, "Deadlock Store");
            throw;
        }
    }

    /// <summary>
    /// Demonstrates stampede scenario by running multiple concurrent requests.
    /// </summary>
    public async Task<List<List<ResourceExample>>> SimulateStampedeAsync(int numberOfTasks = 5, bool useDeadlockProne = false)
    {
        var storeName = useDeadlockProne ? "Deadlock Store" : "Normal Store";
        _logger.Log($"🚀 Starting stampede simulation with {numberOfTasks} concurrent tasks using {storeName}!", LogLevel.Information, "Stampede Simulator");

        var tasks = new List<Task<List<ResourceExample>>>();

        for (int i = 0; i < numberOfTasks; i++)
        {
            var taskNumber = i + 1;
            _logger.Log($"Creating task {taskNumber}/{numberOfTasks}", LogLevel.Debug, "Stampede Simulator");

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
            _logger.Log($"Waiting for all {numberOfTasks} tasks to complete...", LogLevel.Information, "Stampede Simulator");
            var results = await Task.WhenAll(tasks);
            _logger.Log($"✅ Stampede simulation completed! All {numberOfTasks} tasks finished successfully.", LogLevel.Information, "Stampede Simulator");
            return results.ToList();
        }
        catch (Exception ex)
        {
            _logger.Log($"💥 Stampede simulation failed: {ex.Message}", LogLevel.Error, "Stampede Simulator");
            throw;
        }
    }

    /// <summary>
    /// Gets simple resources without delay for comparison.
    /// </summary>
    public List<ResourceExample> GetResourcesSimple()
    {
        _logger.Log("Getting resources with simple/fast method", LogLevel.Information, "Simple Method");

        try
        {
            var resources = _simpleStore.GetRandomResourcesSimple();
            _logger.Log($"Got {resources.Count} resources instantly", LogLevel.Information, "Simple Method");
            return resources;
        }
        catch (Exception ex)
        {
            _logger.Log($"Error in simple fetch: {ex.Message}", LogLevel.Error, "Simple Method");
            throw;
        }
    }
}
