using System.Collections.ObjectModel;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Host.Services;

namespace Scrubbler.Host.Presentation.Plugins;

internal partial class AvailablePluginsViewModel : ObservableObject
{
    #region Properties

    public ObservableCollection<PluginMetadataViewModel> Plugins { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBusy))]
    private bool _isInstalling;

    public bool IsBusy => _manager.IsFetchingPlugins || IsInstalling;

    private readonly IPluginManager _manager;

    #endregion Properties

    public AvailablePluginsViewModel(IPluginManager manager)
    {
        _manager = manager;
        _manager.IsFetchingPluginsChanged += Manager_IsFetchingPluginsChanged;
        _manager.PluginUninstalled += Manager_PluginUninstalled;
        Refresh();
    }

    public void Refresh()
    {
        Plugins.Clear();

        foreach (var meta in _manager.AvailablePlugins)
        {
            var installed = _manager.InstalledPlugins
                .Any(p => string.Equals(p.GetType().FullName, meta.Id, StringComparison.OrdinalIgnoreCase));

            if (!installed)
            {
                var vm = new PluginMetadataViewModel(meta);
                vm.InstallRequested += OnInstallRequested;
                Plugins.Add(vm);
            }
            // if installed → skip (updates handled elsewhere)
        }
    }

    private async void OnInstallRequested(object? sender, PluginManifestEntry meta)
    {
        if (IsInstalling)
            return;

        IsInstalling = true;
        try
        {
            await _manager.InstallAsync(meta);
            Refresh();
        }
        finally
        {
            IsInstalling = false;
        }
    }

    private void Manager_IsFetchingPluginsChanged(object? sender, bool e)
    {
        OnPropertyChanged(nameof(IsBusy));
        if (!e)
            Refresh();
    }

    private void Manager_PluginUninstalled(object? sender, EventArgs e)
    {
        Refresh();
    }
}
