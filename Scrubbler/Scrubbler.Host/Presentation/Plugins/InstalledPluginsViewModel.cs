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
        {
            plugin.Dispose();
        }

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

    private async void OnUninstallRequested(object? sender, string pluginID)
    {
        await _manager.UninstallAsync(_manager.InstalledPlugins.Where(p => p.Id == pluginID).First());
        Refresh();
    }

    private async void OnUpdateRequested(object? sender, string pluginID)
    {
        var pluginManifest = _manager.AvailablePlugins.Where(p => p.Id == pluginID).FirstOrDefault();
        if (pluginManifest == null)
            return;

        if (sender is InstalledPluginViewModel vm)
        {
            vm.Dispose();
            Plugins.Remove(vm);
        }

        var pluginFolder = await _manager.UpdateAsync(pluginManifest);
        if (pluginFolder == null)
            return;

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Directory.Delete(pluginFolder, recursive: true);
        await _manager.InstallAsync(pluginManifest);

        Refresh();
    }
}
