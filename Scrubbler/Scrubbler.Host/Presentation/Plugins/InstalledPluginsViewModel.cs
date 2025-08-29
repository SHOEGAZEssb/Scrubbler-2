using System.Collections.ObjectModel;
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
            var vm = new InstalledPluginViewModel(plugin);
            vm.UninstallRequested += OnUninstallRequested;
            Plugins.Add(vm);
        }
    }

    private async void OnUninstallRequested(object? sender, IPlugin plugin)
    {
        await _manager.UninstallAsync(plugin);
        Refresh();
    }
}
