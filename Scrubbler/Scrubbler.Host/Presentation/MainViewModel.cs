using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Host.Presentation.Accounts;
using Scrubbler.Host.Presentation.Logging;
using Scrubbler.Host.Presentation.Navigation;
using Scrubbler.Host.Presentation.Plugins;
using Scrubbler.Host.Services;
using Scrubbler.Host.Services.Logging;
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

    public MainViewModel(IServiceProvider services)
    {
        // static pages
        Items.Add(new NavigationItemViewModel("Home", new SymbolIconSource() { Symbol = Symbol.Home }, new HomeViewModel()));
        Items.Add(new NavigationItemViewModel("Accounts", new SymbolIconSource() { Symbol = Symbol.Account }, services.GetRequiredService<AccountsViewModel>()));
        Items.Add(new NavigationItemViewModel("Plugin Manager", new SymbolIconSource() { Symbol = Symbol.Admin }, services.GetRequiredService<PluginManagerViewModel>()));

        // plugins group
        var pluginsGroup = new NavigationItemViewModel("Plugins", new SymbolIconSource { Symbol = Symbol.AllApps });

        // plugins
        var manager = services.GetRequiredService<IPluginManager>();
        foreach (var plugin in manager.InstalledPlugins.Where(p => p is IScrobblePlugin))
        {
            pluginsGroup.Children.Add(new PluginNavigationItemViewModel(plugin));
        }

        Items.Add(pluginsGroup);

        Items.Add(new NavigationItemViewModel("Logs", new SymbolIconSource() { Symbol = Symbol.Document }, services.GetRequiredService<LogViewModel>()));

        SelectedItem = Items.FirstOrDefault();
    }
}
