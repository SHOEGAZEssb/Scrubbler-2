namespace Scrubbler.Abstractions.Services;

public sealed class DialogService : IDialogService
{
    private FrameworkElement? _rootElement;

    public void InitializeXamlRoot(FrameworkElement rootElement)
    {
        _rootElement = rootElement;
    }

    /// <summary>
    /// Shows a modal dialog for the given content.
    /// </summary>
    /// <param name="dialog">The <see cref="ContentDialog"/> to display.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="ContentDialogResult"/> indicating how the user dismissed the dialog.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the service has not been initialized with a root element.</exception>
    public async Task<ContentDialogResult> ShowDialogAsync(ContentDialog dialog)
    {
        if (_rootElement == null)
            throw new InvalidOperationException("DialogService is not initialized. Call InitializeXamlRoot with the root element before showing dialogs.");

        dialog.XamlRoot = _rootElement.XamlRoot;
        return await dialog.ShowAsync();
    }
}
