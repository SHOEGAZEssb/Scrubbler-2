using CommunityToolkit.Common;
using CommunityToolkit.Mvvm.Input;
using Scrubbler.Abstractions.Services;
using Scrubbler.Host.Services;

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

    private readonly IUpdateManagerService _manager;
    private readonly IDialogService _dialogService;

    #endregion Properties

    #region Construction

    public AboutSettingsCategoryViewModel(IWritableOptions<UserConfig> config, IUpdateManagerService manager, IDialogService dialogService)
        : base(config)
    {
        _manager = manager;
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
            // check
            var update = await _manager.CheckForUpdateAsync(CancellationToken.None);
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
                var updateProgress = new UpdateInProgressDialog(_manager, update);
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
