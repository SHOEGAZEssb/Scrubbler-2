using Scrubbler.Abstractions.Plugin;
using Scrubbler.Host.Presentation.Plugins;
using Scrubbler.Host.Services;

namespace Scrubbler.Host.Presentation.Navigation;

internal sealed class PluginNavigationItemViewModel(IPlugin plugin, IPluginManager manager) : NavigationItemViewModel(plugin.Name, plugin.Icon, MakeHostViewModel(plugin, manager))
{
    public IPlugin Plugin { get; } = plugin;

    private static object MakeHostViewModel(IPlugin plugin, IPluginManager manager)
    {
        if (plugin is IScrobblePlugin sp)
            return new ScrobblePluginHostViewModel(sp, manager);

        return null; // todo log throw?
    }
}
