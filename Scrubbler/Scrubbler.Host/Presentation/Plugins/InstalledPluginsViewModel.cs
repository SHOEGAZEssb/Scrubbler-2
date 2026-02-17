using System.Collections.ObjectModel;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Host.Services;

namespace Scrubbler.Host.Presentation.Plugins;

internal partial class InstalledPluginsViewModel : ObservableObject
{
    #region Properties

    public ObservableCollection<InstalledPluginViewModel> Plugins { get; } = [];

    public event EventHandler? IsBusyChanged;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBusy))]
    private bool _isUninstalling;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBusy))]
    private bool _isUpdating;

    public bool IsBusy => IsUninstalling || IsUpdating;

    private readonly IPluginManager _manager;

    #endregion Properties

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
        var pluginManifest = _manager.AvailablePlugins.FirstOrDefault(p => p.Name == plugin.Name);
        if (pluginManifest == null)
            return false;

        return new Version(pluginManifest.Version) > plugin.Version;
    }

    private async void OnUninstallRequested(object? sender, string pluginID)
    {
        if (IsUninstalling)
            return;

        IsUninstalling = true;
        try
        {
            await _manager.UninstallAsync(_manager.InstalledPlugins.First(p => p.Id == pluginID));
            Refresh();
        }
        finally
        {
            IsUninstalling = false;
        }
    }

    private async void OnUpdateRequested(object? sender, string pluginID)
    {
        if (IsUpdating)
            return;

        var pluginManifest = _manager.AvailablePlugins.FirstOrDefault(p => p.Id == pluginID);
        if (pluginManifest == null)
            return;

        IsUpdating = true;
        try
        {
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
        finally
        {
            IsUpdating = false;
        }
    }
}
