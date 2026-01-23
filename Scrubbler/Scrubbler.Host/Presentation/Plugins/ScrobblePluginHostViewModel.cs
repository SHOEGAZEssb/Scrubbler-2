using CommunityToolkit.Mvvm.Input;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Services;
using Scrubbler.Host.Services;

namespace Scrubbler.Host.Presentation.Plugins;

internal partial class ScrobblePluginHostViewModel : PluginHostViewModelBase
{
    #region Properties

    public override bool IsBusy => base.IsBusy || PluginViewModel.IsBusy;

    public IScrobblePluginViewModel PluginViewModel { get; }

    public bool ShowScrobbleBar => PluginViewModel.ReadyForScrobbling;

    private bool CanPreview => PluginViewModel.CanScrobble;

    private bool CanScrobble => PluginViewModel.CanScrobble && _pluginManager.IsAnyAccountPluginScrobbling;

    #endregion Properties

    #region Construction

    public ScrobblePluginHostViewModel(IScrobblePlugin plugin, IPluginManager manager, IUserFeedbackService feedbackService, IDialogService dialogService)
        : base(plugin, manager, feedbackService, dialogService)
    {
        PluginViewModel = (_plugin.GetViewModel() as IScrobblePluginViewModel)!; // todo: log in case this is ever null (should not happen)

        PluginViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(IScrobblePluginViewModel.CanScrobble))
            {
                ScrobbleCommand.NotifyCanExecuteChanged();
                PreviewCommand.NotifyCanExecuteChanged();
            }
            else if (e.PropertyName == nameof(IScrobblePluginViewModel.ReadyForScrobbling))
                OnPropertyChanged(nameof(ShowScrobbleBar));
            else if (e.PropertyName == nameof(IScrobblePluginViewModel.IsBusy))
                OnPropertyChanged(nameof(IsBusy));
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
