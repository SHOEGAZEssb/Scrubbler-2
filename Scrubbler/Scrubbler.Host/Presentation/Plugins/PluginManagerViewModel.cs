using Scrubbler.Abstractions.Plugin;
using Scrubbler.Host.Services;

namespace Scrubbler.Host.Presentation.Plugins;

internal class PluginManagerViewModel
{
    public InstalledPluginsViewModel Installed { get; }
    public AvailablePluginsViewModel Available { get; }

    public PluginManagerViewModel(IPluginManager manager)
    {
        Installed = new InstalledPluginsViewModel(manager);
        Available = new AvailablePluginsViewModel(manager);
    }

    // optional: a refresh-all command
    public void Refresh()
    {
        Installed.Refresh();
        Available.Refresh();
    }
}
