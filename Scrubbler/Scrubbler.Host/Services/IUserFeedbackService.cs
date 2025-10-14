namespace Scrubbler.Host.Services;

public interface IUserFeedbackService
{
    void ShowSuccess(string message, TimeSpan? duration = null);
    void ShowError(string message, TimeSpan? duration = null);
    void ShowInfo(string message, TimeSpan? duration = null);
}
