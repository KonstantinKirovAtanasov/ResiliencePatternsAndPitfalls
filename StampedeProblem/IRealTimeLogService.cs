namespace StampedeProblem;

/// <summary>
/// Interface for real-time logging service that can broadcast log messages.
/// </summary>
public interface IRealTimeLogService
{
    public event Action<LogEntry>? LogEntryAdded;

    /// <summary>
    /// Logs a message with the specified level and source.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="level">The log level (default: Information).</param>
    /// <param name="source">The source of the log message (optional).</param>
    void Log(string message, LogLevelInternal level = LogLevelInternal.Information, string? source = null, bool highlighted = false);

    /// <summary>
    /// Clears all log entries.
    /// </summary>
    void Clear();
}

/// <summary>
/// Represents a log entry with timestamp and metadata.
/// </summary>
public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;
    public LogLevelInternal Level { get; set; }
    public string Source { get; set; } = string.Empty;
    public int ThreadId { get; set; }
    public bool HighLighted { get; set; } = false;

    public string GetLevelClass()
    {
        return Level switch
        {
            LogLevelInternal.Error => "text-danger",
            LogLevelInternal.Warning => "text-warning",
            LogLevelInternal.Information => "text-primary",
            LogLevelInternal.Debug => "text-muted",
            _ => "text-dark"
        };
    }

    public string GetLevelIcon()
    {
        return Level switch
        {
            LogLevelInternal.Error => "❌",
            LogLevelInternal.Warning => "⚠️",
            LogLevelInternal.Information => "ℹ️",
            LogLevelInternal.Debug => "🔍",
            _ => "📝"
        };
    }
}

/// <summary>
/// Log levels for the real-time logging service.
/// </summary>
public enum LogLevelInternal
{
    Debug,
    Information,
    Warning,
    Error,
    HighLighted // Custom level for highlighted messages
}
