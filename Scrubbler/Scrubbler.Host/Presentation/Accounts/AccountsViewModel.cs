using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Scrubbler.Abstractions.Plugin.Account;
using Scrubbler.Host.Services;

namespace Scrubbler.Host.Presentation.Accounts;

internal class AccountsViewModel : ObservableObject
{
    public ObservableCollection<AccountPluginViewModel> Accounts { get; } = [];

    private readonly IPluginManager _pluginManager;
    private readonly IWritableOptions<UserConfig> _config;

    public AccountsViewModel(IPluginManager pluginManager, IWritableOptions<UserConfig> config)
    {
        _pluginManager = pluginManager;
        _config = config;
        _pluginManager.PluginInstalled += PluginManager_PluginInstalled;
        Accounts.CollectionChanged += Accounts_CollectionChanged;
        Refresh();
    }

    private void PluginManager_PluginInstalled(object? sender, EventArgs e)
    {
        Refresh();
    }

    private void Refresh()
    {
        Accounts.Clear();

        foreach (var plugin in _pluginManager.InstalledPlugins.OfType<IAccountPlugin>())
            Accounts.Add(new AccountPluginViewModel(plugin, _config));
    }

    private void Accounts_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (AccountPluginViewModel item in e.NewItems!)
            {
                item.RequestedIsUsingAccountFunctionsChange += Account_RequestedIsUsingAccountFunctionsChange;
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset)
        {
            foreach (AccountPluginViewModel item in e.OldItems!)
            {
                item.RequestedIsUsingAccountFunctionsChange -= Account_RequestedIsUsingAccountFunctionsChange;
            }
        }
    }

    private async void Account_RequestedIsUsingAccountFunctionsChange(object? sender, bool e)
    {
        if (sender is AccountPluginViewModel vm)
        {
            var newSetting = e ? vm.Name : null;

            // update setting
            await _config.UpdateAsync(current =>
            {
                var updated = current with
                {
                    AccountFunctionsPluginID = newSetting
                };

                return updated;
            });

            foreach (var account in Accounts)
            {
                // notify
                account.UpdateIsUsingAccountFunctions();
            }
        }
    }
}
