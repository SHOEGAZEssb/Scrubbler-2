namespace Scrubbler.Host.Services;

/// <summary>
/// Uno implementation of IUserFeedbackService using Snackbars.
/// </summary>
public class UserFeedbackService : IUserFeedbackService
{
    private InfoBar? _infoBar;

    public void AttachInfoBar(InfoBar infoBar) => _infoBar = infoBar;

    public void ShowSuccess(string message, TimeSpan? duration = null) =>
        Show(message, InfoBarSeverity.Success, duration ?? TimeSpan.FromSeconds(5));

    public void ShowError(string message, TimeSpan? duration = null) =>
        Show(message, InfoBarSeverity.Error, duration ?? TimeSpan.FromSeconds(5));

    public void ShowInfo(string message, TimeSpan? duration = null) =>
        Show(message, InfoBarSeverity.Informational, duration ?? TimeSpan.FromSeconds(5));

    private async void Show(string message, InfoBarSeverity severity, TimeSpan duration)
    {
        if (_infoBar == null)
            return;

        _infoBar.Message = message;
        _infoBar.Severity = severity;
        _infoBar.IsOpen = true;

        await Task.Delay(duration);
        _infoBar.IsOpen = false;
    }
}
