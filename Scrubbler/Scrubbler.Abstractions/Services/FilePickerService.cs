using Windows.Storage.Pickers;

namespace Scrubbler.Abstractions.Services;

public sealed class FilePickerService(IWindowHandleProvider? windowHandleProvider = null) : IFilePickerService
{
    private readonly IWindowHandleProvider? _windowHandleProvider = windowHandleProvider;

    public async Task<StorageFile?> PickFileAsync(
        IReadOnlyList<string> fileTypes,
        PickerLocationId startLocation = PickerLocationId.DocumentsLibrary)
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = startLocation
        };

        foreach (var type in fileTypes)
            picker.FileTypeFilter.Add(type);

        InitializeIfNeeded(picker);

        return await picker.PickSingleFileAsync();
    }

    /// <summary>
    /// Opens a file-save dialog.
    /// </summary>
    public async Task<StorageFile?> SaveFileAsync(
        string suggestedFileName,
        IReadOnlyDictionary<string, IReadOnlyList<string>> fileTypeChoices,
        PickerLocationId startLocation = PickerLocationId.DocumentsLibrary)
    {
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = startLocation,
            SuggestedFileName = suggestedFileName
        };

        foreach (var (description, extensions) in fileTypeChoices)
            picker.FileTypeChoices.Add(description, [.. extensions]);

        InitializeIfNeeded(picker);

        return await picker.PickSaveFileAsync();
    }

    private void InitializeIfNeeded(object picker)
    {
        if (!OperatingSystem.IsWindows())
            return;

        if (_windowHandleProvider is null)
            throw new InvalidOperationException(
                "Windows file pickers require a window handle. " +
                "Register IWindowHandleProvider in the UI project.");

        WinRT.Interop.InitializeWithWindow.Initialize(
            picker,
            _windowHandleProvider.GetWindowHandle());
    }
}

