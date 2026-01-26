using CommunityToolkit.Mvvm.Input;
using Scrubbler.Abstractions.Services;
using Scrubbler.Host.Presentation.Plugins;
using Scrubbler.Host.Updates;

namespace Scrubbler.Host.Presentation.Settings;

internal partial class SettingsViewModel : ObservableObject
{
    #region Properties

    private readonly IWritableOptions<UserConfig> _userConfigOptions;
    private readonly IDialogService _dialogService;

    #endregion Properties

    #region Construction

    public SettingsViewModel(IWritableOptions<UserConfig> userConfigOptions, IDialogService dialogService)
    {
        _userConfigOptions = userConfigOptions;
        _dialogService = dialogService;

        if (_userConfigOptions.Value.CheckForUpdatesOnStartup)
            _ = CheckForUpdates();
    }

    #endregion Construction

    [RelayCommand]
    private async Task CheckForUpdates()
    {
        var http = new HttpClient();
        var source = new JsonManifestUpdateSource(new Uri("http://localhost:8000/manifest.json"), http);

        var updateManager = new UpdateManager(http, source);

        // check
        var update = await updateManager.CheckForUpdateAsync(CancellationToken.None);
        if (update is null)
        {
            if (!_userConfigOptions.Value.CheckForUpdatesOnStartup)
            {
                var noUpdate = new ContentDialog
                {
                    Title = "No Updates",
                    Content = "You are running the latest version of the Scrubbler.",
                    CloseButtonText = "OK"
                };
                await _dialogService.ShowDialogAsync(noUpdate);
            }

            return;
        }

        // prompt
        var d = new UpdateDialog(update);
        if (await _dialogService.ShowDialogAsync(d) == ContentDialogResult.Primary)
        {
            var updateProgress = new UpdateInProgressDialog(updateManager, update);
            await _dialogService.ShowDialogAsync(updateProgress);
        }
    }
}
