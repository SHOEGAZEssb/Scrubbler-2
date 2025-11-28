using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Host.Services;

namespace Scrubbler.Host.Presentation.Plugins;

internal partial class AutoScrobblePluginHostViewModel : ScrobblePluginHostViewModelBase
{
    #region Properties

    [ObservableProperty]
    private IAutoScrobblePluginViewModel _pluginViewModel;

    #endregion Properties

    public AutoScrobblePluginHostViewModel(IAutoScrobblePlugin plugin, IPluginManager pluginManager, IUserFeedbackService userFeedbackService, IDialogService dialogService)
        : base(plugin, pluginManager, userFeedbackService, dialogService)
    {
        _pluginViewModel = (plugin.GetViewModel() as IAutoScrobblePluginViewModel)!; // todo: log in case this is ever null (should not happen)
        _pluginViewModel.ScrobblesDetected += PluginViewModel_ScrobblesDetected;
    }

    private async void PluginViewModel_ScrobblesDetected(object? sender, IEnumerable<ScrobbleData> e)
    {
        await ScrobbleToPlugins(e);
        _userFeedbackService.ShowSuccess("Successfully scrobbled");
    }
}
