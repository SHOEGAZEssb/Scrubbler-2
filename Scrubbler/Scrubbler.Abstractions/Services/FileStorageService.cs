using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;

namespace Scrubbler.Abstractions.Services;

public sealed class FileStorageService : IFileStorageService
{
    /// <summary>
    /// Writes text to the specified file, overwriting existing content.
    /// </summary>
    public async Task WriteTextAsync(
        StorageFile file,
        string content,
        Encoding? encoding = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        var bytes = (encoding ?? Encoding.UTF8).GetBytes(content);
        await WriteBytesInternalAsync(file, bytes, cancellationToken);
    }

    /// <summary>
    /// Writes lines to the specified file, overwriting existing content.
    /// </summary>
    public async Task WriteLinesAsync(
        StorageFile file,
        IEnumerable<string> lines,
        Encoding? encoding = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(lines);

        var text = string.Join(Environment.NewLine, lines);
        await WriteTextAsync(file, text, encoding, cancellationToken);
    }

    private static async Task WriteBytesInternalAsync(
        StorageFile file,
        byte[] bytes,
        CancellationToken cancellationToken)
    {
        using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
        cancellationToken.ThrowIfCancellationRequested();

        using var output = stream.GetOutputStreamAt(0);

        await output.WriteAsync(bytes.AsBuffer());
        await output.FlushAsync();

        // truncate if new content is shorter than old content
        if (stream.Size != (ulong)bytes.Length)
            stream.Size = (ulong)bytes.Length;
    }
}
