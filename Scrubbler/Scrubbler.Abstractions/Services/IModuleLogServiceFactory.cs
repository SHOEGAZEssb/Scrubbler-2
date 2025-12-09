namespace Scrubbler.Abstractions.Services;

public interface IModuleLogServiceFactory
{
    ILogService Create(string moduleName);
}
