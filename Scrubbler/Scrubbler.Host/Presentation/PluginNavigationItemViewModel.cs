using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scrubbler.Abstractions;

namespace Scrubbler.Host.Presentation;

internal sealed class PluginNavigationItemViewModel : NavigationItemViewModel
{
    public IPlugin Plugin { get; }

    public PluginNavigationItemViewModel(IPlugin plugin)
        : base(plugin.Name, plugin.Icon, plugin.GetViewModel())
    {
        Plugin = plugin;
    }
}
