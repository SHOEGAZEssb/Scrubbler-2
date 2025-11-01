namespace Scrubbler.Host.Models;

/// <summary>
/// Represents a log message entry.
/// </summary>
/// <param name="Timestamp">The time when the log message was created.</param>
/// <param name="Level">The severity level of the log message.</param>
/// <param name="Module">The module or component that generated the log message.</param>
/// <param name="Message">The log message text.</param>
/// <param name="Exception">The exception associated with this log message, if any.</param>
public record LogMessage(
    DateTime Timestamp,
    LogLevel Level,
    string Module,
    string Message,
    Exception? Exception = null
);
