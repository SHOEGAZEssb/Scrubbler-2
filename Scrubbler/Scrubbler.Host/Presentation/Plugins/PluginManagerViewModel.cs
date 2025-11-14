using Scrubbler.Host.Services;

namespace Scrubbler.Host.Presentation.Plugins;

internal class PluginManagerViewModel(IPluginManager manager)
{
    public InstalledPluginsViewModel Installed { get; } = new InstalledPluginsViewModel(manager);
    public AvailablePluginsViewModel Available { get; } = new AvailablePluginsViewModel(manager);

    // optional: a refresh-all command
    public void Refresh()
    {
        Installed.Refresh();
        Available.Refresh();
    }
}
