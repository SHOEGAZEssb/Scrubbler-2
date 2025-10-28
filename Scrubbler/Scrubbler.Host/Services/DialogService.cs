namespace Scrubbler.Host.Services;

internal sealed class DialogService : IDialogService
{
    private FrameworkElement? _rootElement;

    internal void InitializeXamlRoot(FrameworkElement rootElement)
    {
        _rootElement = rootElement;
    }

    public async Task<ContentDialogResult> ShowDialogAsync(ContentDialog dialog)
    {
        if (_rootElement == null)
            throw new InvalidOperationException("DialogService is not initialized. Call InitializeXamlRoot with the root element before showing dialogs.");

        dialog.XamlRoot = _rootElement.XamlRoot;
        return await dialog.ShowAsync();
    }
}
