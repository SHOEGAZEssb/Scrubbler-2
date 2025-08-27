using Scrubbler.Abstractions;

namespace Scrubbler.Host.Presentation.Navigation;

internal sealed class PluginNavigationItemViewModel : NavigationItemViewModel
{
    public IPlugin Plugin { get; }

    public PluginNavigationItemViewModel(IPlugin plugin)
        : base(plugin.Name, plugin.Icon, plugin.GetViewModel())
    {
        Plugin = plugin;
    }
}
