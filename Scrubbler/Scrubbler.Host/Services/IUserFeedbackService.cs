namespace Scrubbler.Host.Services;

/// <summary>
/// Service for displaying user feedback messages (success, error, info).
/// </summary>
public interface IUserFeedbackService
{
    /// <summary>
    /// Displays a success message to the user.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="duration">The duration to show the message. If <c>null</c>, defaults to 5 seconds.</param>
    void ShowSuccess(string message, TimeSpan? duration = null);
    
    /// <summary>
    /// Displays an error message to the user.
    /// </summary>
    /// <param name="message">The error message to display.</param>
    /// <param name="duration">The duration to show the message. If <c>null</c>, defaults to 5 seconds.</param>
    void ShowError(string message, TimeSpan? duration = null);
    
    /// <summary>
    /// Displays an informational message to the user.
    /// </summary>
    /// <param name="message">The informational message to display.</param>
    /// <param name="duration">The duration to show the message. If <c>null</c>, defaults to 5 seconds.</param>
    void ShowInfo(string message, TimeSpan? duration = null);
}
