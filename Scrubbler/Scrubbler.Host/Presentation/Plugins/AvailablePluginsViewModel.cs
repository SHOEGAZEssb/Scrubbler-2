using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Host.Services;

namespace Scrubbler.Host.Presentation.Plugins;

internal partial class AvailablePluginsViewModel : ObservableObject
{
    #region Properties

    public ObservableCollection<PluginMetadataViewModel> Plugins { get; } = [];

    public IEnumerable<PluginMetadataViewModel> FilteredPlugins => Plugins
        .Where(p => (p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) || (p.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
        .Where(p => !ShowOnlyCompatible || p.IsCompatible);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilteredPlugins))]
    private string _searchText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilteredPlugins))]
    private bool _showOnlyCompatible = true;

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
        RefreshList();
    }

    private void RefreshList()
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

    [RelayCommand]
    private async Task Refresh()
    {
        if (IsBusy)
            return;

        await _manager.RefreshAvailablePluginsAsync();
    }

    private async void OnInstallRequested(object? sender, PluginManifestEntry meta)
    {
        if (IsInstalling)
            return;

        IsInstalling = true;
        try
        {
            await _manager.InstallAsync(meta);
            RefreshList();
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
            RefreshList();
    }

    private void Manager_PluginUninstalled(object? sender, EventArgs e)
    {
        RefreshList();
    }
}
