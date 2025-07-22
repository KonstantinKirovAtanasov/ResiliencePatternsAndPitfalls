using System.Collections.Concurrent;

namespace WellKnownProblems.Services;

/// <summary>
/// Real-time logging service that broadcasts log messages to connected Blazor components.
/// </summary>
public class RealTimeLogService
{
    private readonly ConcurrentQueue<LogEntry> _logEntries = new();
    private const int MaxLogEntries = 1000;

    public event Action<LogEntry>? LogEntryAdded;

    /// <summary>
    /// Adds a log entry and notifies subscribers.
    /// </summary>
    public void Log(string message, LogLevel level = LogLevel.Information, string? source = null)
    {
        var entry = new LogEntry
        {
            Timestamp = DateTime.Now,
            Message = message,
            Level = level,
            Source = source ?? "Unknown",
            ThreadId = Thread.CurrentThread.ManagedThreadId
        };

        _logEntries.Enqueue(entry);

        // Keep only the last MaxLogEntries
        while (_logEntries.Count > MaxLogEntries)
        {
            _logEntries.TryDequeue(out _);
        }

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
            Level = LogLevel.Information,
            Source = "System",
            ThreadId = 0
        });
    }

    /// <summary>
    /// Clears all log entries (alias for Clear method).
    /// </summary>
    public void ClearLogs()
    {
        Clear();
    }
}

/// <summary>
/// Represents a log entry with timestamp and metadata.
/// </summary>
public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;
    public LogLevel Level { get; set; }
    public string Source { get; set; } = string.Empty;
    public int ThreadId { get; set; }

    public string GetLevelClass()
    {
        return Level switch
        {
            LogLevel.Error => "text-danger",
            LogLevel.Warning => "text-warning",
            LogLevel.Information => "text-primary",
            LogLevel.Debug => "text-muted",
            _ => "text-dark"
        };
    }

    public string GetLevelIcon()
    {
        return Level switch
        {
            LogLevel.Error => "❌",
            LogLevel.Warning => "⚠️",
            LogLevel.Information => "ℹ️",
            LogLevel.Debug => "🔍",
            _ => "📝"
        };
    }
}

public enum LogLevel
{
    Debug,
    Information,
    Warning,
    Error
}
