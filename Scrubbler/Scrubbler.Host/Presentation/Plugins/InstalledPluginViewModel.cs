using CommunityToolkit.Mvvm.Input;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Plugin.Account;

namespace Scrubbler.Host.Presentation.Plugins;

internal partial class InstalledPluginViewModel(IPlugin plugin) : ObservableObject
{
    public event EventHandler<IPlugin>? UninstallRequested;

    public string Name => _plugin.Name;
    public string Description => _plugin.Description;
    public Version Version => _plugin.Version;
    public IconSource? Icon => /*_plugin.Icon*/ null;

    public string PluginType
    {
        get
        {
            if (_plugin is IAccountPlugin)
                return "Account Plugin";
            if (_plugin is IScrobblePlugin || _plugin is IAutoScrobblePlugin)
                return "Scrobble Plugin";

            return "Plugin";
        }
    }

    private readonly IPlugin _plugin = plugin;

    [RelayCommand]
    private void Uninstall()
    {
        UninstallRequested?.Invoke(this, _plugin);
    }
}
