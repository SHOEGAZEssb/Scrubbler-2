using Microsoft.UI.Xaml.Controls;

namespace Scrubbler.Abstractions;

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
    PlatformSupport SupportedPlatforms { get; }
    IPluginViewModel GetViewModel();
    IconSource? Icon {  get; }
}
