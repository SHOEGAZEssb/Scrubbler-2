using System.Collections.ObjectModel;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Host.Services;

namespace Scrubbler.Host.Presentation.Plugins;

internal class AvailablePluginsViewModel : ObservableObject
{
    public ObservableCollection<PluginMetadataViewModel> Plugins { get; } = new();

    public bool IsFetchingPlugins => _manager.IsFetchingPlugins;

    private readonly IPluginManager _manager;

    public AvailablePluginsViewModel(IPluginManager manager)
    {
        _manager = manager;
        _manager.IsFetchingPluginsChanged += Manager_IsFetchingPluginsChanged;
        Refresh();
    }

    public void Refresh()
    {
        Plugins.Clear();

        foreach (var meta in _manager.AvailablePlugins)
        {
            var vm = new PluginMetadataViewModel(meta);
            vm.InstallRequested += OnInstallRequested;
            Plugins.Add(vm);
        }
    }

    private async void OnInstallRequested(object? sender, PluginManifestEntry meta)
    {
        await _manager.InstallAsync(meta);
        Refresh();
    }

    private void Manager_IsFetchingPluginsChanged(object? sender, bool e)
    {
        OnPropertyChanged(nameof(IsFetchingPlugins));
        Refresh();
    }
}
