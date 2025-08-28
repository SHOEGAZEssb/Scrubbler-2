using System.Collections.ObjectModel;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Host.Services;

namespace Scrubbler.Host.Presentation.Accounts;
internal class AccountsViewModel : ObservableObject
{
    public ObservableCollection<AccountPluginViewModel> Accounts { get; } = [];

    public AccountsViewModel(IPluginManager pluginManager)
    {
        foreach (var plugin in pluginManager.InstalledPlugins.OfType<IAccountPlugin>())
            Accounts.Add(new AccountPluginViewModel(plugin));
    }
}
