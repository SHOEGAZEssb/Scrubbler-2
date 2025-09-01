using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Host.Presentation.Accounts;
using Scrubbler.Host.Presentation.Logging;
using Scrubbler.Host.Presentation.Navigation;
using Scrubbler.Host.Presentation.Plugins;
using Scrubbler.Host.Services;

namespace Scrubbler.Host.Presentation;

internal partial class MainViewModel : ObservableObject
{
    #region Properties

    public ObservableCollection<NavigationItemViewModel> Items { get; } = [];

    public NavigationItemViewModel? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
            {
                // Whenever SelectedItem changes, notify dependent properties
                OnPropertyChanged(nameof(CurrentViewModel));
            }
        }
    }
    private NavigationItemViewModel? _selectedItem;

    public object? CurrentViewModel => SelectedItem?.Content;

    private readonly NavigationItemViewModel _pluginGroup;
    private readonly IPluginManager _pluginManager;

    #endregion Properties

    public MainViewModel(IServiceProvider services)
    {
        // static pages
        Items.Add(new NavigationItemViewModel("Home", new SymbolIconSource() { Symbol = Symbol.Home }, new HomeViewModel()));
        Items.Add(new NavigationItemViewModel("Accounts", new SymbolIconSource() { Symbol = Symbol.Account }, services.GetRequiredService<AccountsViewModel>()));
        Items.Add(new NavigationItemViewModel("Plugin Manager", new SymbolIconSource() { Symbol = Symbol.Admin }, services.GetRequiredService<PluginManagerViewModel>()));

        // plugins group
        _pluginGroup = new NavigationItemViewModel("Plugins", new SymbolIconSource { Symbol = Symbol.AllApps });

        // plugins
        _pluginManager = services.GetRequiredService<IPluginManager>();
        _pluginManager.PluginInstalled += PluginManager_PluginInstalled;
        RefreshPluginList();

        Items.Add(_pluginGroup);

        Items.Add(new NavigationItemViewModel("Logs", new SymbolIconSource() { Symbol = Symbol.Document }, services.GetRequiredService<LogViewModel>()));

        SelectedItem = Items.FirstOrDefault();
    }

    private void PluginManager_PluginInstalled(object? sender, EventArgs e)
    {
        RefreshPluginList();
    }

    private void RefreshPluginList()
    {
        _pluginGroup.Children.Clear();

        foreach (var plugin in _pluginManager.InstalledPlugins.Where(p => p is IScrobblePlugin))
        {
            _pluginGroup.Children.Add(new PluginNavigationItemViewModel(plugin, _pluginManager));
        }
    }
}
