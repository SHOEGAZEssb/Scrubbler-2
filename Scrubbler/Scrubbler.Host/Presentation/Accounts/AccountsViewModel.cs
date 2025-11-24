using System.Collections.ObjectModel;
using Scrubbler.Abstractions.Plugin.Account;
using Scrubbler.Host.Services;

namespace Scrubbler.Host.Presentation.Accounts;
internal class AccountsViewModel : ObservableObject
{
    public ObservableCollection<AccountPluginViewModel> Accounts { get; } = [];

    private readonly IPluginManager _pluginManager;

    public AccountsViewModel(IPluginManager pluginManager)
    {
        _pluginManager = pluginManager;
        _pluginManager.PluginInstalled += PluginManager_PluginInstalled;
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
            Accounts.Add(new AccountPluginViewModel(plugin));
    }
}
