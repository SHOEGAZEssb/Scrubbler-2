namespace Scrubbler.Host.Services;

/// <summary>
/// Shows modal dialogs in a cross-platform way.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows a modal dialog for the given content.
    /// </summary>
    Task<ContentDialogResult> ShowDialogAsync(ContentDialog dialog);
}
