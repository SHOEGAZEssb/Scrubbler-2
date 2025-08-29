using Microsoft.UI.Xaml.Controls;
using Scrubbler.Abstractions.Logging;

namespace Scrubbler.Abstractions.Plugin;

[Flags]
public enum PlatformSupport
{
    Windows = 1,
    Mac = 2,
    Linux = 4,
    All = Windows | Mac | Linux
}

public interface IPlugin
{
    string Name { get; }
    string Description { get; }
    Version Version { get; }
    PlatformSupport SupportedPlatforms { get; }
    IPluginViewModel GetViewModel();
    IconSource? Icon { get; }
    ILogService LogService { get; set; }
}
