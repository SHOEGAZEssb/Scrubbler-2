namespace Scrubbler.Host.Presentation.Logging;

internal partial class LogLevelFilterViewModel(LogLevel level) : ObservableObject
{
    #region Properties

    [ObservableProperty]
    private bool _isEnabled = true;

    public LogLevel Level { get; } = level;

    #endregion Properties

    public bool PassesFilter(LogMessage message)
    {
        if (message.Level == Level)
            return IsEnabled;

        return true;
    }
}
