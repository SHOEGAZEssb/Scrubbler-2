using Scrubbler.Abstractions.Services;

namespace Scrubbler.Host.Services.Logging;

internal class ModuleLogServiceFactory(HostLogService hostLog) : IModuleLogServiceFactory
{
    private readonly HostLogService _hostLog = hostLog;

    public ILogService Create(string moduleName) => new ModuleLogService(_hostLog, moduleName);
}
