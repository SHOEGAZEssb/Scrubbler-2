using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Host.Services;

namespace Scrubbler.Host.Presentation.Plugins;
internal partial class ScrobblePluginHostViewModel : ObservableObject
{
    #region Properties

    [ObservableProperty]
    private IScrobblePluginViewModel _pluginViewModel;

    private readonly IPlugin _plugin;
    private readonly IPluginManager _pluginManager;

    private bool CanScrobble => PluginViewModel.CanScrobble;

    #endregion Properties

    #region Construction

    public ScrobblePluginHostViewModel(IScrobblePlugin plugin, IPluginManager manager)
    {
        _plugin = plugin;
        _pluginManager = manager;
        
        _pluginViewModel = (_plugin.GetViewModel() as IScrobblePluginViewModel)!; // todo: log in case this is ever null (should not happen)

        _pluginViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(IScrobblePluginViewModel.CanScrobble))
            {
                ScrobbleCommand.NotifyCanExecuteChanged();
                PreviewCommand.NotifyCanExecuteChanged();
            }
        };

    }

    #endregion Construction

    [RelayCommand(CanExecute = nameof(CanScrobble))]
    private async Task Scrobble()
    {
        if (PluginViewModel is IScrobblePluginViewModel spv)
            await ScrobbleToPlugins(await spv.GetScrobblesAsync());
    }

    private async Task ScrobbleToPlugins(IEnumerable<ScrobbleData> scrobbles)
    {
        foreach (var account in _pluginManager.InstalledPlugins.OfType<IAccountPlugin>().Where(p => p.IsScrobblingEnabled))
        {
            await account.ScrobbleAsync(scrobbles);
        }
    }

    [RelayCommand(CanExecute = nameof(CanScrobble))]
    private async Task Preview()
    {

    }
}
