using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scrubbler.Abstractions;

namespace Scrubbler.Host.Presentation.Accounts;
internal class AccountsViewModel : ObservableObject
{
    public ObservableCollection<AccountPluginViewModel> Accounts { get; } = [];

    public AccountsViewModel()
    {
        //foreach (var plugin in plugins)
        //    Accounts.Add(new AccountPluginViewModel(plugin));
    }
}
