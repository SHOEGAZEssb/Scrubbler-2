using System.Text;

namespace Scrubbler.Abstractions.Services;

/// <summary>
/// Provides basic text file write helpers.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Writes text to the specified file, overwriting existing content.
    /// </summary>
    Task WriteTextAsync(
        StorageFile file,
        string content,
        Encoding? encoding = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes lines to the specified file, overwriting existing content.
    /// </summary>
    Task WriteLinesAsync(
        StorageFile file,
        IEnumerable<string> lines,
        Encoding? encoding = null,
        CancellationToken cancellationToken = default);
}
