using System;

namespace UxPlayDesktop.Models;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}

public class LogEntry
{
    public DateTime Timestamp { get; } = DateTime.Now;
    public LogLevel Level { get; set; } = LogLevel.Info;
    public string Message { get; set; } = string.Empty;

    public string FormattedTime => Timestamp.ToString("HH:mm:ss.fff");

    public string BadgeColor => Level switch
    {
        LogLevel.Debug => "#6C757D",
        LogLevel.Info => "#0D6EFD",
        LogLevel.Warning => "#FFC107",
        LogLevel.Error => "#DC3545",
        _ => "#ADB5BD"
    };
}
