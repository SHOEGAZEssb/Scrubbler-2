namespace Scrubbler.Abstractions.Services;

/// <summary>
/// Provides logging functionality for plugins.
/// </summary>
public interface ILogService
{
    /// <summary>
    /// Logs a debug-level message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void Debug(string message);

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void Info(string message);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void Warn(string message);

    /// <summary>
    /// Logs an error message, optionally including exception details.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    /// <param name="ex">The exception associated with this error, if any.</param>
    void Error(string message, Exception? ex = null);

    /// <summary>
    /// Logs a critical error message, optionally including exception details.
    /// </summary>
    /// <param name="message">The critical error message to log.</param>
    /// <param name="ex">The exception associated with this critical error, if any.</param>
    void Critical(string message, Exception? ex = null);
}

