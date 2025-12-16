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

    public ObservableCollection<NavigationItemViewModelBase> Items { get; } = [];

    public NavigationItemViewModelBase? SelectedItem
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
    private NavigationItemViewModelBase? _selectedItem;

    public object? CurrentViewModel => SelectedItem?.Content;

    private readonly MenuNavigationItemViewModel _pluginGroup;
    private readonly IPluginManager _pluginManager;
    private readonly IUserFeedbackService _userFeedbackService;
    private readonly IDialogService _dialogService;

    #endregion Properties

    public MainViewModel(IServiceProvider services)
    {
        // static pages
        Items.Add(new MenuNavigationItemViewModel("Home", new SymbolIconSource() { Symbol = Symbol.Home }, new HomeViewModel()));
        Items.Add(new MenuNavigationItemViewModel("Accounts", new SymbolIconSource() { Symbol = Symbol.Account }, services.GetRequiredService<AccountsViewModel>()));
        Items.Add(new MenuNavigationItemViewModel("Plugin Manager", new SymbolIconSource() { Symbol = Symbol.Admin }, services.GetRequiredService<PluginManagerViewModel>()));

        // plugins group
        _pluginGroup = new MenuNavigationItemViewModel("Plugins", new SymbolIconSource { Symbol = Symbol.AllApps });

        // plugins
        _pluginManager = services.GetRequiredService<IPluginManager>();
        _pluginManager.PluginInstalled += PluginManager_PluginInstalled;
        _pluginManager.PluginUnloading += PluginManager_PluginUnloading;
        _userFeedbackService = services.GetRequiredService<IUserFeedbackService>();
        _dialogService = services.GetRequiredService<IDialogService>();
        RefreshPluginList();

        Items.Add(_pluginGroup);

        Items.Add(new MenuNavigationItemViewModel("Logs", new SymbolIconSource() { Symbol = Symbol.Document }, services.GetRequiredService<LogViewModel>()));

        SelectedItem = Items.FirstOrDefault();
    }

    private void PluginManager_PluginUnloading(object? sender, IPlugin e)
    {
        var nav = _pluginGroup.Children.Where(n => n .Title == e.Name).FirstOrDefault();
        if (nav != null)
            _pluginGroup.Children.Remove(nav);
    }

    private void PluginManager_PluginInstalled(object? sender, EventArgs e)
    {
        RefreshPluginList();
    }

    private void RefreshPluginList()
    {
        _pluginGroup.Children.Clear();

        foreach (var plugin in _pluginManager.InstalledPlugins.Where(p => p is IScrobblePlugin || p is IAutoScrobblePlugin))
        {
            _pluginGroup.Children.Add(new PluginNavigationItemViewModel(plugin, _pluginManager, _userFeedbackService, _dialogService));
        }
    }
}
