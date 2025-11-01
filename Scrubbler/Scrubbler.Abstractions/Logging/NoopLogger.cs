namespace Scrubbler.Abstractions.Logging;

/// <summary>
/// A no-operation implementation of <see cref="ILogService"/> that discards all log messages.
/// </summary>
/// <remarks>
/// Useful for testing or when logging is not needed.
/// </remarks>
public class NoopLogger : ILogService
{
    /// <summary>
    /// Does nothing. All log messages are discarded.
    /// </summary>
    /// <param name="message">Ignored.</param>
    public void Critical(string message, Exception? ex = null)
    {
    }

    /// <summary>
    /// Does nothing. All log messages are discarded.
    /// </summary>
    /// <param name="message">Ignored.</param>
    public void Debug(string message)
    {
    }

    /// <summary>
    /// Does nothing. All log messages are discarded.
    /// </summary>
    /// <param name="message">Ignored.</param>
    /// <param name="ex">Ignored.</param>
    public void Error(string message, Exception? ex = null)
    {
    }

    /// <summary>
    /// Does nothing. All log messages are discarded.
    /// </summary>
    /// <param name="message">Ignored.</param>
    public void Info(string message)
    {
    }

    /// <summary>
    /// Does nothing. All log messages are discarded.
    /// </summary>
    /// <param name="message">Ignored.</param>
    public void Warn(string message)
    {
    }
}
