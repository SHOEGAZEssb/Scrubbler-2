namespace Scrubbler.Host.Services.Logging;

internal class HostLogService : IHostedService
{
    public event Action<LogMessage>? MessageLogged;

    internal void Write(LogLevel level, string module, string message, Exception? ex = null)
    {
        var entry = new LogMessage(DateTime.UtcNow, level, module, message, ex);
        MessageLogged?.Invoke(entry);
    }

    public Task StartAsync(CancellationToken ct) => Task.CompletedTask;
    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
