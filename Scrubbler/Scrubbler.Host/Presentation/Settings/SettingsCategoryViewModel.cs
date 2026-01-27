namespace Scrubbler.Host.Presentation.Settings;

internal abstract class SettingsCategoryViewModel(IWritableOptions<UserConfig> config) : ObservableObject
{
    #region Properties

    public abstract string Name { get; }

    protected readonly IWritableOptions<UserConfig> _config = config;

    #endregion Properties
}
