namespace Scrubbler.Abstractions.Logging;

public class NoopLogger : ILogService
{
    public void Critical(string message, Exception? ex = null)
    {
    }

    public void Debug(string message)
    {
    }

    public void Error(string message, Exception? ex = null)
    {
    }

    public void Info(string message)
    {
    }

    public void Warn(string message)
    {
    }
}
