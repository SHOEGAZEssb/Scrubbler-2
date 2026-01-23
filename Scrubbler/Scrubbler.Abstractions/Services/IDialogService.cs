namespace Scrubbler.Abstractions.Services;

/// <summary>
/// Shows modal dialogs in a cross-platform way.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows a modal dialog for the given content.
    /// </summary>
    /// <param name="dialog">The <see cref="ContentDialog"/> to display.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="ContentDialogResult"/> indicating how the user dismissed the dialog.</returns>
    Task<ContentDialogResult> ShowDialogAsync(ContentDialog dialog);
}
