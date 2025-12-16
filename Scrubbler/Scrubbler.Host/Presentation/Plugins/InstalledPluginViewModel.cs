using CommunityToolkit.Mvvm.Input;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Plugin.Account;
using Scrubbler.Host.Helper;

namespace Scrubbler.Host.Presentation.Plugins;

internal partial class InstalledPluginViewModel(IPlugin plugin, bool canBeUpdated) : ObservableObject
{
    #region Properties

    public event EventHandler<IPlugin>? UninstallRequested;
    public event EventHandler<IPlugin>? UpdateRequested;

    public string Name => _plugin.Name;
    public string Description => _plugin.Description;
    public Version Version => _plugin.Version;
    public ImageSource? Icon => _icon ??= PluginIconHelper.LoadPluginIcon(_plugin);
    private ImageSource? _icon;

    public bool CanBeUpdated { get; } = canBeUpdated;

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

    #endregion Properties

    [RelayCommand]
    private void Uninstall()
    {
        UninstallRequested?.Invoke(this, _plugin);
    }

    [RelayCommand(CanExecute = nameof(CanBeUpdated))]
    private void Update()
    {
        UpdateRequested?.Invoke(this, _plugin);
    }
}
