using StampedeProblem;
using System.Collections.Concurrent;

namespace StampedeProblemExamples.Services;


/// <summary>
/// Real-time logging service that broadcasts log messages to connected Blazor components.
/// </summary>
public class RealTimeLogService : IRealTimeLogService
{
    private readonly ConcurrentQueue<LogEntry> _logEntries = new();
    private const int MaxLogEntries = 1000;

    public event Action<LogEntry>? LogEntryAdded;

    /// <summary>
    /// Adds a log entry and notifies subscribers.
    /// </summary>
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

        _logEntries.Enqueue(entry);
        while (_logEntries.Count > MaxLogEntries) _logEntries.TryDequeue(out _);
        LogEntryAdded?.Invoke(entry);
    }

    /// <summary>
    /// Gets all current log entries.
    /// </summary>
    public IEnumerable<LogEntry> GetLogEntries()
    {
        return _logEntries.ToArray();
    }

    /// <summary>
    /// Clears all log entries.
    /// </summary>
    public void Clear()
    {
        while (_logEntries.TryDequeue(out _))
        {
            // Clear all entries
        }

        LogEntryAdded?.Invoke(new LogEntry
        {
            Timestamp = DateTime.Now,
            Message = "Log cleared",
            Level = LogLevelInternal.Information,
            Source = "System",
            ThreadId = 0
        });
    }
}