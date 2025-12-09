using System.Reflection;
using Scrubbler.Abstractions.Services;

namespace Scrubbler.Abstractions.Plugin;

public abstract class PluginBase : IPlugin
{
    #region Properties

    public string Name { get; }

    public string Description { get; }

    public PlatformSupport SupportedPlatforms { get; }

    public Version Version => GetType().Assembly.GetName().Version!;

    protected readonly ILogService _logService;

    #endregion Properties

    #region Construction

    protected PluginBase(IModuleLogServiceFactory logFactory)
    {
        var attribute = GetType().GetCustomAttribute<PluginMetadataAttribute>();
        if (attribute == null)
            throw new InvalidOperationException($"{GetType().Name} must have [PluginMetadata] attribute.");

        Name = attribute.Name;
        Description = attribute.Description;
        SupportedPlatforms = attribute.SupportedPlatforms;
        _logService = logFactory.Create(Name);
    }

    #endregion Construction

    public abstract IPluginViewModel GetViewModel();
}
