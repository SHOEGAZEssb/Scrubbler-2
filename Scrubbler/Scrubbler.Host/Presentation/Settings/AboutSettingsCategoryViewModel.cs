using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Common;
using CommunityToolkit.Mvvm.Input;
using Scrubbler.Abstractions.Services;
using Scrubbler.Host.Updates;

namespace Scrubbler.Host.Presentation.Settings;

internal partial class AboutSettingsCategoryViewModel : SettingsCategoryViewModel
{
    #region Properties

    public override string Name { get; } = "About";

    public string CurrentVersion { get; }

    public bool CheckForUpdatesOnStartup
    {
        get => _config.Value.CheckForUpdatesOnStartup;
        set
        {
            if (CheckForUpdatesOnStartup != value)
            {
                _ = _config.UpdateAsync(current =>
                {
                    var updated = current with
                    {
                        CheckForUpdatesOnStartup = value
                    };

                    return updated;
                }).GetResultOrDefault();

                OnPropertyChanged();
            }
        }
    }

    public bool CanCheckForUpdates => !IsCheckingForUpdates;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CheckForUpdatesCommand))]
    private bool _isCheckingForUpdates;

    private readonly IDialogService _dialogService;

    #endregion Properties

    #region Construction

    public AboutSettingsCategoryViewModel(IWritableOptions<UserConfig> config, IDialogService dialogService)
        : base(config)
    {
        _dialogService = dialogService;
        var version = Package.Current.Id.Version;
        CurrentVersion = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

        if (_config.Value.CheckForUpdatesOnStartup)
            _ = CheckForUpdates();
    }

    #endregion Construction

    [RelayCommand(CanExecute = nameof(CanCheckForUpdates))]
    private async Task CheckForUpdates()
    {
        IsCheckingForUpdates = true;

        try
        {
            var http = new HttpClient();
            var source = new JsonManifestUpdateSource(new Uri("http://localhost:8000/manifest.json"), http);

            var updateManager = new UpdateManager(http, source);

            // check
            var update = await updateManager.CheckForUpdateAsync(CancellationToken.None);
            if (update is null)
            {
                if (!_config.Value.CheckForUpdatesOnStartup)
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
        catch (Exception ex)
        {

        }
        finally
        {
            IsCheckingForUpdates = false;
        }
    }
}
