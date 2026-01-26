using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Scrubbler.Abstractions.Services;
using Scrubbler.Host.Presentation.Plugins;
using Scrubbler.Host.Updates;

namespace Scrubbler.Host.Presentation.Settings;

internal partial class SettingsViewModel : ObservableObject
{
    #region Properties

    public ObservableCollection<SettingsCategoryViewModel> Categories { get; }

    private readonly IWritableOptions<UserConfig> _userConfigOptions;

    #endregion Properties

    #region Construction

    public SettingsViewModel(IWritableOptions<UserConfig> userConfigOptions, IDialogService dialogService)
    {
        _userConfigOptions = userConfigOptions;
        Categories =
        [
            new AboutSettingsCategoryViewModel(_userConfigOptions, dialogService)
        ];
    }

    #endregion Construction
}
