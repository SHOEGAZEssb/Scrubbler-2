using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Host.Services;

namespace Scrubbler.Host.Presentation.Plugins;

internal class InstalledPluginsViewModel : ObservableObject
{
    public ObservableCollection<InstalledPluginViewModel> Plugins { get; } = [];

    private readonly IPluginManager _manager;

    public InstalledPluginsViewModel(IPluginManager manager)
    {
        _manager = manager;
        _manager.PluginInstalled += Manager_PluginInstalled;
        _manager.IsFetchingPluginsChanged += Manager_IsFetchingPluginsChanged;
        Refresh();
    }

    private void Manager_IsFetchingPluginsChanged(object? sender, bool e)
    {
        if (!e)
            Refresh();
    }

    private void Manager_PluginInstalled(object? sender, EventArgs e)
    {
        Refresh();
    }

    public void Refresh()
    {
        foreach (var plugin in Plugins)
            plugin.UninstallRequested -= OnUninstallRequested;

        Plugins.Clear();

        foreach (var plugin in _manager.InstalledPlugins)
        {
            var vm = new InstalledPluginViewModel(plugin, HasPluginUpdate(plugin));
            vm.UninstallRequested += OnUninstallRequested;
            vm.UpdateRequested += OnUpdateRequested;
            Plugins.Add(vm);
        }
    }

    private bool HasPluginUpdate(IPlugin plugin)
    {
        var pluginManifest = _manager.AvailablePlugins.Where(p => p.Name == plugin.Name).FirstOrDefault();
        if (pluginManifest == null)
            return false;

        return new Version(pluginManifest.Version) > plugin.Version;
    }

    private async void OnUninstallRequested(object? sender, IPlugin plugin)
    {
        await _manager.UninstallAsync(plugin);
        Refresh();
    }

    private async void OnUpdateRequested(object? sender, IPlugin e)
    {
        var pluginManifest = _manager.AvailablePlugins.Where(p => p.Name == e.Name).FirstOrDefault();
        if (pluginManifest == null)
            return;

        Plugins.Remove((sender as InstalledPluginViewModel)!);
        await _manager.UpdateAsync(e, pluginManifest);
        Refresh();
    }
}
