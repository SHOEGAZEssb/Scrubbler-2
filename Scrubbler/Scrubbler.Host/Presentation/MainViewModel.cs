using System.Collections.ObjectModel;
using Scrubbler.Abstractions;
using Scrubbler.Host.Presentation.Accounts;
using Scrubbler.Host.Presentation.Navigation;
using Scrubbler.Plugin.ManualScrobbler;

namespace Scrubbler.Host.Presentation;

internal partial class MainViewModel : ObservableObject
{
    public ObservableCollection<NavigationItemViewModel> Items { get; } = [];

    public NavigationItemViewModel? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
            {
                // Whenever SelectedItem changes, notify dependent properties
                OnPropertyChanged(nameof(IsScrobblePanelVisible));
                OnPropertyChanged(nameof(CurrentViewModel));
            }
        }
    }
    private NavigationItemViewModel? _selectedItem;

    public object? CurrentViewModel => SelectedItem?.Content;

    public bool IsScrobblePanelVisible => SelectedItem is PluginNavigationItemViewModel { Plugin: IScrobblePlugin };

    public MainViewModel()
    {
        // static pages
        Items.Add(new NavigationItemViewModel("Home", new SymbolIconSource() { Symbol = Symbol.Home }, new HomeViewModel()));
        Items.Add(new NavigationItemViewModel("Accounts", new SymbolIconSource() { Symbol = Symbol.Account }, new AccountsViewModel()));
        Items.Add(new NavigationItemViewModel("Plugin Manager", new SymbolIconSource() { Symbol = Symbol.Admin }, new PluginManagerViewModel()));

        // plugins group
        var pluginsGroup = new NavigationItemViewModel("Plugins", new SymbolIconSource { Symbol = Symbol.AllApps });

        // plugins
        var plugins = new List<IPlugin> { new ManualScrobblePlugin() };
        foreach (var plugin in plugins)
        {
            pluginsGroup.Children.Add(new PluginNavigationItemViewModel(plugin));
        }

        Items.Add(pluginsGroup);

        SelectedItem = Items.FirstOrDefault();
    }
}
