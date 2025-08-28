namespace Scrubbler.Abstractions.Logging;


/// <summary>
/// Provides logging functionality for plugins.
/// </summary>
public interface ILogService
{
    void Debug(string message);
    void Info(string message);
    void Warn(string message);
    void Error(string message, Exception? ex = null);
    void Critical(string message, Exception? ex = null);
}

