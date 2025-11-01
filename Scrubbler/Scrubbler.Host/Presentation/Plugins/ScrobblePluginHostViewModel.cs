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
    private readonly IUserFeedbackService _userFeedbackService;
    private readonly IDialogService _dialogService;

    private bool CanPreview => PluginViewModel.CanScrobble;
    private bool CanScrobble => PluginViewModel.CanScrobble && _pluginManager.IsAnyAccountPluginScrobbling;

    #endregion Properties

    #region Construction

    public ScrobblePluginHostViewModel(IScrobblePlugin plugin, IPluginManager manager, IUserFeedbackService feedbackService, IDialogService dialogService)
    {
        _plugin = plugin;
        _pluginManager = manager;
        _userFeedbackService = feedbackService;
        _dialogService = dialogService;

        _pluginViewModel = (_plugin.GetViewModel() as IScrobblePluginViewModel)!; // todo: log in case this is ever null (should not happen)

        _pluginViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(IScrobblePluginViewModel.CanScrobble))
            {
                ScrobbleCommand.NotifyCanExecuteChanged();
                PreviewCommand.NotifyCanExecuteChanged();
            }
        };

        _pluginManager.IsAnyAccountPluginScrobblingChanged += (s, e) =>
        {
            ScrobbleCommand.NotifyCanExecuteChanged();
        };
    }

    #endregion Construction

    [RelayCommand(CanExecute = nameof(CanScrobble))]
    private async Task Scrobble()
    {
        if (PluginViewModel is IScrobblePluginViewModel spv)
            await ScrobbleToPlugins(await spv.GetScrobblesAsync());

        _userFeedbackService.ShowSuccess("Scrobbled!");
    }

    private async Task ScrobbleToPlugins(IEnumerable<ScrobbleData> scrobbles)
    {
        foreach (var account in _pluginManager.InstalledPlugins.OfType<IAccountPlugin>().Where(p => p.IsScrobblingEnabled))
        {
            await account.ScrobbleAsync(scrobbles);
        }
    }

    [RelayCommand(CanExecute = nameof(CanPreview))]
    private async Task Preview()
    {
        if (PluginViewModel is IScrobblePluginViewModel spv)
        {
            var d = new ScrobblePreviewDialog(await spv.GetScrobblesAsync());
            await _dialogService.ShowDialogAsync(d);
        }
    }
}
