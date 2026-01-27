using System.Collections.ObjectModel;
using Scrubbler.Abstractions.Services;
using Scrubbler.Host.Services;

namespace Scrubbler.Host.Presentation.Settings;

internal partial class SettingsViewModel : ObservableObject
{
    #region Properties

    public ObservableCollection<SettingsCategoryViewModel> Categories { get; }

    private readonly IWritableOptions<UserConfig> _userConfigOptions;

    #endregion Properties

    #region Construction

    public SettingsViewModel(IWritableOptions<UserConfig> userConfigOptions, IUpdateManagerService updateManager, IDialogService dialogService)
    {
        _userConfigOptions = userConfigOptions;
        Categories =
        [
            new AboutSettingsCategoryViewModel(_userConfigOptions, updateManager, dialogService)
        ];
    }

    #endregion Construction
}
