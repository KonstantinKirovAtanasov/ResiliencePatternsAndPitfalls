using System.Collections.Concurrent;

namespace StampedeProblem.Stores;

/// <summary>
/// Represents a simple resource store that returns random resources with simulated delay.
/// </summary>
public class SimpleResourceStore(int delayMs = 200, IRealTimeLogService? logger = null)
{
    private readonly Random _random = new();
    private readonly int _delayMs = delayMs;
    protected readonly IRealTimeLogService? _logger = logger;

    /// <summary>
    /// Gets 20 random resources with simulated delay.
    /// </summary>
    /// <returns>A collection of 20 random resources.</returns>
    public virtual async Task<List<ResourceExample>> GetRandomResourcesAsync()
    {
        var threadId = Thread.CurrentThread.ManagedThreadId;
        var message = $"Thread {threadId}: Starting to fetch 20 random resources...";

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
        _logger?.Log(message, LogLevelInternal.Information, "SimpleResourceStore");
        var resources = new List<ResourceExample>();

        for (int i = 0; i < 10; i++)
        {
            var randomName = ResourceStaticData.ResourceNames[_random.Next(ResourceStaticData.ResourceNames.Length)];
            await Task.Delay(_delayMs);
            resources.Add(new ResourceExample { Name = randomName });
        }

        var completedMessage = $"Thread {threadId}: Completed fetching 20 resources after {_delayMs}ms";
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {completedMessage}");
        _logger?.Log(completedMessage, LogLevelInternal.Information, "SimpleResourceStore");
        resources.ForEach(p => _logger?.Log(p?.ToString() ?? "", LogLevelInternal.Debug, nameof(SimpleResourceStore)));

        return resources;
    }
}
