using Scrubbler.Abstractions.Logging;

namespace Scrubbler.Host.Services.Logging;

internal class ModuleLogService : ILogService
{
    private readonly HostLogService _hostLog;
    private readonly string _moduleName;

    public ModuleLogService(HostLogService hostLog, string pluginName)
    {
        _hostLog = hostLog;
        _moduleName = pluginName;
    }

    public void Debug(string message) => _hostLog.Write(LogLevel.Debug, _moduleName, message);
    public void Info(string message) => _hostLog.Write(LogLevel.Information, _moduleName, message);
    public void Warn(string message) => _hostLog.Write(LogLevel.Warning, _moduleName, message);
    public void Error(string message, Exception? ex = null) =>
        _hostLog.Write(LogLevel.Error, _moduleName, message, ex);
    public void Critical(string message, Exception? ex = null) =>
        _hostLog.Write(LogLevel.Critical, _moduleName, message, ex);
}
