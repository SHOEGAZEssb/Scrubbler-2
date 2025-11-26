namespace Scrubbler.Host.Presentation;

internal class SettingsViewModel(IWritableOptions<UserConfig> userConfigOptions) : ObservableObject
{
    #region Properties

    private readonly IWritableOptions<UserConfig> _userConfigOptions = userConfigOptions;

    #endregion Properties
}
