using System.Collections.Concurrent;
using System.Diagnostics;

namespace StampedeProblem.Stores;

/// <summary>
/// Represents a deadlock-prone resource store that demonstrates thread safety issues.
/// This implementation reuses SimpleResourceStore logic but is vulnerable to deadlocks.
/// </summary>
/// <param name="delayMs">Delay in milliseconds for operations.</param>
/// <param name="logger">The real-time logging service.</param>
public class DeadlockProneResourceStore(int delayMs = 300, IRealTimeLogService? logger = null)
    : SimpleResourceStore(delayMs, logger)
{
    private static readonly object _lock1 = new object();
    private static readonly object _lock2 = new object();

    /// <summary>
    /// Gets 20 random resources with simulated delay - VULNERABLE TO DEADLOCK!
    /// This method reuses SimpleResourceStore logic but creates deadlock scenarios.
    /// </summary>
    /// <returns>A collection of 20 random resources.</returns>
    public override async Task<(List<ResourceExample> result, long elapsedMilliseconds, long elapsedTicks)> GetRandomResourcesAsync()
    {
        var threadId = Thread.CurrentThread.ManagedThreadId;
        var enteringMessage = $"Thread {threadId}: Entering DeadlockProneResourceStore - attempting to acquire locks";
        _logger?.Log(enteringMessage, LogLevelInternal.Warning, "DeadlockProneResourceStore");

        Stopwatch stopwatch = Stopwatch.StartNew();
        return await Task.Run(async () =>
        {
            Task<(List<ResourceExample> result, long elapsedMilliseconds, long elapsedTicks)> resourceTask = null!;

            if (threadId % 2 == 0)
            {
                var attemptLock1Message = $"Thread {threadId}: Attempting to acquire lock1...";
                _logger?.Log(attemptLock1Message, LogLevelInternal.Warning, "DeadlockProneResourceStore");

                lock (_lock1)
                {
                    var acquiredLock1Message = $"Thread {threadId}: Acquired lock1, waiting before lock2...";
                    _logger?.Log(acquiredLock1Message, LogLevelInternal.Warning, "DeadlockProneResourceStore");

                    Thread.Sleep(150);
                    var attemptLock2Message = $"Thread {threadId}: Attempting to acquire lock2...";
                    _logger?.Log(attemptLock2Message, LogLevelInternal.Warning, "DeadlockProneResourceStore");

                    lock (_lock2)
                    {
                        var bothLocksMessage = $"Thread {threadId}: Acquired both locks, delegating to SimpleResourceStore";
                        _logger?.Log(bothLocksMessage, LogLevelInternal.Information, "DeadlockProneResourceStore");
                        resourceTask = base.GetRandomResourcesAsync();
                    }
                }
            }
            else
            {
                var attemptLock2Message = $"Thread {threadId}: Attempting to acquire lock2...";
                _logger?.Log(attemptLock2Message, LogLevelInternal.Warning, "DeadlockProneResourceStore");

                lock (_lock2)
                {
                    var acquiredLock2Message = $"Thread {threadId}: Acquired lock2, waiting before lock1...";
                    _logger?.Log(acquiredLock2Message, LogLevelInternal.Warning, "DeadlockProneResourceStore");

                    Thread.Sleep(1500);
                    var attemptLock1Message = $"Thread {threadId}: Attempting to acquire lock1...";
                    _logger?.Log(attemptLock1Message, LogLevelInternal.Error, "DeadlockProneResourceStore");
                    lock (_lock1)
                    {
                        Thread.Sleep(1500);
                        var bothLocksMessage = $"Thread {threadId}: Acquired both locks, delegating to SimpleResourceStore";
                        _logger?.Log(bothLocksMessage, LogLevelInternal.Information, "DeadlockProneResourceStore");
                        resourceTask = base.GetRandomResourcesAsync();
                    }
                }
            }

            return ((await resourceTask).result, stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks);
        });
    }
}