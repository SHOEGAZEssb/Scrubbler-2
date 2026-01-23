using Windows.Storage.Pickers;

namespace Scrubbler.Abstractions.Services;

public interface IFilePickerService
{
    /// <summary>
    /// Opens a file-open dialog.
    /// </summary>
    Task<StorageFile?> PickFileAsync(
        IReadOnlyList<string> fileTypes,
        PickerLocationId startLocation = PickerLocationId.DocumentsLibrary);

    /// <summary>
    /// Opens a file-save dialog.
    /// </summary>
    Task<StorageFile?> SaveFileAsync(
        string suggestedFileName,
        IReadOnlyDictionary<string, IReadOnlyList<string>> fileTypeChoices,
        PickerLocationId startLocation = PickerLocationId.DocumentsLibrary);
}
