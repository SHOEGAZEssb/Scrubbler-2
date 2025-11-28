using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;

namespace Scrubbler.Host.Services;

/// <summary>
/// Uno implementation of IUserFeedbackService using Snackbars.
/// </summary>
public class UserFeedbackService : IUserFeedbackService
{
    private InfoBar? _infoBar;

    /// <summary>
    /// Attaches an <see cref="InfoBar"/> control to this service for displaying messages.
    /// </summary>
    /// <param name="infoBar">The InfoBar control to use for displaying messages.</param>
    public void AttachInfoBar(InfoBar infoBar) => _infoBar = infoBar;

    /// <summary>
    /// Displays a success message to the user.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="duration">The duration to show the message. If <c>null</c>, defaults to 5 seconds.</param>
    public void ShowSuccess(string message, TimeSpan? duration = null) =>
        Show(message, InfoBarSeverity.Success, duration ?? TimeSpan.FromSeconds(5));

    /// <summary>
    /// Displays an error message to the user.
    /// </summary>
    /// <param name="message">The error message to display.</param>
    /// <param name="duration">The duration to show the message. If <c>null</c>, defaults to 5 seconds.</param>
    public void ShowError(string message, TimeSpan? duration = null) =>
        Show(message, InfoBarSeverity.Error, duration ?? TimeSpan.FromSeconds(5));

    /// <summary>
    /// Displays an informational message to the user.
    /// </summary>
    /// <param name="message">The informational message to display.</param>
    /// <param name="duration">The duration to show the message. If <c>null</c>, defaults to 5 seconds.</param>
    public void ShowInfo(string message, TimeSpan? duration = null) =>
        Show(message, InfoBarSeverity.Informational, duration ?? TimeSpan.FromSeconds(5));

    private async void Show(string message, InfoBarSeverity severity, TimeSpan duration)
    {
        if (_infoBar == null)
            return;

        var dispatcher = Window.Current?.DispatcherQueue;
        if (dispatcher == null)
            return;

        await dispatcher.EnqueueAsync(() =>
        {
            _infoBar.Message = message;
            _infoBar.Severity = severity;
            _infoBar.IsOpen = true;
        });

        await Task.Delay(duration);
        
        await dispatcher.EnqueueAsync(() =>
        {
            _infoBar.IsOpen = false;
        });
    }
}
