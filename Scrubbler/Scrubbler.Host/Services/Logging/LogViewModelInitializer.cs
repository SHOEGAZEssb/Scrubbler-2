using Scrubbler.Host.Presentation.Logging;

namespace Scrubbler.Host.Services.Logging;
internal class LogViewModelInitializer : IHostedService
{
    public LogViewModelInitializer(LogViewModel log)
    {
        // just taking the dependency ensures it's constructed
        _ = log;
    }

    public Task StartAsync(CancellationToken ct) => Task.CompletedTask;
    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
