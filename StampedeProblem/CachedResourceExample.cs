using StampedeProblem;
using StampedeProblem.Stores;

namespace StampedeProblem;

/// <summary>
/// Simple console example demonstrating cached resource store functionality.
/// </summary>
public class CachedResourceExample
{
    public static async Task RunExample()
    {
        Console.WriteLine("=== Cached Resource Store Example ===");
        Console.WriteLine("Demonstrates caching with 10-minute expiration to prevent stampede problems.\n");

        // Create a simple logger
        var logger = new ConsoleLogger();

        // Create cached resource store with internal cache
        using var cachedStore = new CachedResourceStore(delayMs: 1000, logger: logger);

        // Simulate stampede scenario with multiple concurrent requests
        Console.WriteLine("🚀 Simulating stampede with 5 concurrent requests...\n");

        List<Task<(List<ResourceExample> result, long elapsedMilliseconds, long elapsedTicks)>> tasks = new ();
        DateTime startTime = DateTime.Now;

        // Create 5 concurrent requests
        for (int i = 0; i < 5; i++)
        {
            int taskId = i + 1;
            tasks.Add(cachedStore.GetRandomResourcesAsync().ContinueWith(p =>
            {
                Console.WriteLine($"[Task {taskId}] Completed with {p.Result.result.Count} resources");
                return p.Result;
            }));
        }

        // Wait for all tasks to complete
        var results = await Task.WhenAll(tasks);
        var totalDuration = (DateTime.Now - startTime).TotalMilliseconds;

        Console.WriteLine($"\n✅ All tasks completed in {totalDuration:F0}ms");
        Console.WriteLine($"📊 Results: {results.Length} tasks, each got {results[0].result.Count} resources");

        // Demonstrate cache hit by making another request
        Console.WriteLine("\n🔄 Making another request (should hit cache)...");
        var cacheTestStart = DateTime.Now;
        var cachedResult = await cachedStore.GetRandomResourcesAsync();
        var cacheTestDuration = (DateTime.Now - cacheTestStart).TotalMilliseconds;

        Console.WriteLine($"⚡ Cache hit completed in {cacheTestDuration:F0}ms (much faster!)");

        // Clear cache and test again
        Console.WriteLine("\n🧹 Clearing cache and testing again...");
        cachedStore.ClearCache();

        var noCacheStart = DateTime.Now;
        var noCacheResult = await cachedStore.GetRandomResourcesAsync();
        var noCacheDuration = (DateTime.Now - noCacheStart).TotalMilliseconds;

        Console.WriteLine($"🐌 After cache clear: {noCacheDuration:F0}ms (slower, as expected)");

        Console.WriteLine("\n✨ Example completed! Cache is automatically disposed.");
    }
}

/// <summary>
/// Simple console logger implementation.
/// </summary>
public class ConsoleLogger : IRealTimeLogService
{
    public event Action<LogEntry>? LogEntryAdded;

    public void Log(string message, LogLevelInternal level = LogLevelInternal.Information, string? source = null, bool highlighted = false)
    {
        var entry = new LogEntry
        {
            Timestamp = DateTime.Now,
            Message = message,
            Level = level,
            Source = source ?? "Unknown",
            ThreadId = Thread.CurrentThread.ManagedThreadId,
            HighLighted = highlighted
        };

        LogEntryAdded?.Invoke(entry);

        // Format console output with color and icon
        var color = GetConsoleColor(level);
        var icon = GetIcon(level);
        var originalColor = Console.ForegroundColor;

        Console.ForegroundColor = color;
        Console.WriteLine($"{icon} [{level}] [{source}] (T{entry.ThreadId}): {message}");
        Console.ForegroundColor = originalColor;
    }

    public void Clear()
    {
        // Nothing to clear for console
    }

    private ConsoleColor GetConsoleColor(LogLevelInternal level) => level switch
    {
        LogLevelInternal.Error => ConsoleColor.Red,
        LogLevelInternal.Warning => ConsoleColor.Yellow,
        LogLevelInternal.Information => ConsoleColor.Cyan,
        LogLevelInternal.Debug => ConsoleColor.Gray,
        _ => ConsoleColor.White
    };

    private string GetIcon(LogLevelInternal level) => level switch
    {
        LogLevelInternal.Error => "❌",
        LogLevelInternal.Warning => "⚠️",
        LogLevelInternal.Information => "ℹ️",
        LogLevelInternal.Debug => "🔍",
        _ => "📝"
    };
}
