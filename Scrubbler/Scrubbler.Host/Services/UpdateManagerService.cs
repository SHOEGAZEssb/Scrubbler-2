using System.Diagnostics;
using System.Security.Cryptography;
using Scrubbler.Host.Updates;

namespace Scrubbler.Host.Services;

/// <summary>
/// Coordinates update check, download, verification, and handoff to the external updater.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="UpdateManagerService"/>.
/// </remarks>
public sealed class UpdateManagerService(HttpClient http, IUpdateSource source, UpdateManagerOptions? options = null) : IUpdateManagerService
{
    private readonly HttpClient _http = http ?? throw new ArgumentNullException(nameof(http));
    private readonly IUpdateSource _source = source ?? throw new ArgumentNullException(nameof(source));
    private readonly UpdateManagerOptions _options = options ?? new UpdateManagerOptions();

    /// <summary>
    /// Checks if an update is available for the current app.
    /// </summary>
    public async Task<UpdateInfo?> CheckForUpdateAsync(CancellationToken ct)
    {
        var currentVersion = _options.CurrentVersionProvider();
        var rid = _options.RuntimeIdentifierProvider();

        return await _source.GetLatestAsync(currentVersion, rid, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Downloads and verifies an update package, then starts the updater and exits the app.
    /// </summary>
    public async Task ApplyUpdateAndRestartAsync(UpdateInfo update, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(update);

        var appDir = _options.AppDirectoryProvider();
        var entryPath = _options.EntryPathProvider();

        var zipPath = await DownloadPackageToTempAsync(update.PackageUri, ct).ConfigureAwait(false);

        var actualSha256 = await ComputeSha256Async(zipPath, ct).ConfigureAwait(false);
        if (!actualSha256.Equals(update.Sha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("update package sha256 mismatch");

        var updaterExeInApp = GetUpdaterExePath(appDir);
        var stagedUpdaterExe = StageUpdaterToTemp(updaterExeInApp);

        var args =
            $"--pid {Environment.ProcessId} " +
            $"--appDir \"{appDir}\" " +
            $"--package \"{zipPath}\" " +
            $"--entry \"{entryPath}\"";

        Process.Start(new ProcessStartInfo
        {
            FileName = stagedUpdaterExe,
            Arguments = args,
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(stagedUpdaterExe) ?? Path.GetTempPath()
        });

        Environment.Exit(0);
    }

    private string GetUpdaterExePath(string appDir)
    {
        var exeName = OperatingSystem.IsWindows() ? _options.UpdaterExeNameWindows : _options.UpdaterExeNameUnix;
        var path = Path.Combine(appDir, _options.UpdaterRelativeDirectory, exeName);

        if (!File.Exists(path))
            throw new FileNotFoundException("Updater executable not found. Expected it at: " + path, path);

        return path;
    }

    private static string StageUpdaterToTemp(string updaterExePath)
    {
        var updaterDir = Path.GetDirectoryName(updaterExePath)
                         ?? throw new InvalidOperationException("Updater directory not found");

        var tempDir = Path.Combine(Path.GetTempPath(), $"scrubbler_updater_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        // copy everything from the updater folder (exe + deps + runtimeconfig etc.)
        foreach (var file in Directory.EnumerateFiles(updaterDir, "*", SearchOption.TopDirectoryOnly))
        {
            var dest = Path.Combine(tempDir, Path.GetFileName(file));
            File.Copy(file, dest, overwrite: true);
        }

        var exeName = OperatingSystem.IsWindows() ? "Scrubbler.Updater.exe" : "Scrubbler.Updater";
        var stagedExe = Path.Combine(tempDir, exeName);

        if (!File.Exists(stagedExe))
            throw new FileNotFoundException("Staged updater executable not found. Expected it at: " + stagedExe, stagedExe);

        return stagedExe;
    }

    private async Task<string> DownloadPackageToTempAsync(Uri uri, CancellationToken ct)
    {
        var tempZip = Path.Combine(Path.GetTempPath(), $"scrubbler_update_{Guid.NewGuid():N}.zip");

        if (uri.Scheme.Equals("file", StringComparison.OrdinalIgnoreCase))
        {
            // allow local end-to-end tests without any server
            var source = uri.LocalPath;
            File.Copy(source, tempZip, overwrite: true);
            return tempZip;
        }

        await using var fs = File.Create(tempZip);
        await using var stream = await _http.GetStreamAsync(uri, ct).ConfigureAwait(false);
        await stream.CopyToAsync(fs, ct).ConfigureAwait(false);

        return tempZip;
    }

    private static async Task<string> ComputeSha256Async(string filePath, CancellationToken ct)
    {
        await using var fs = File.OpenRead(filePath);
        using var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(fs, ct).ConfigureAwait(false);
        return Convert.ToHexString(hash);
    }
}

/// <summary>
/// Configures how <see cref="UpdateManagerService"/> locates paths and names.
/// </summary>
public sealed class UpdateManagerOptions
{
    /// <summary>
    /// Folder relative to the app directory where the updater lives.
    /// </summary>
    public string UpdaterRelativeDirectory { get; init; } = "Updater";

    /// <summary>
    /// Updater executable name on Windows.
    /// </summary>
    public string UpdaterExeNameWindows { get; init; } = "Scrubbler.Updater.exe";

    /// <summary>
    /// Updater executable name on Linux/macOS.
    /// </summary>
    public string UpdaterExeNameUnix { get; init; } = "Scrubbler.Updater";

    /// <summary>
    /// Provides the current app version used for comparison.
    /// </summary>
    public Func<Version> CurrentVersionProvider { get; init; } =
        () => typeof(UpdateManagerService).Assembly.GetName().Version ?? new Version(0, 0, 0);

    /// <summary>
    /// Provides the runtime identifier key used in the manifest (e.g. win-x64, linux-x64, osx-x64).
    /// </summary>
    public Func<string> RuntimeIdentifierProvider { get; init; } =
        () => System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier;

    /// <summary>
    /// Provides the directory the app is running from (portable install root).
    /// </summary>
    public Func<string> AppDirectoryProvider { get; init; } =
        () => AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    /// <summary>
    /// Provides the full path to the currently running process entrypoint.
    /// </summary>
    public Func<string> EntryPathProvider { get; init; } =
        () => Environment.ProcessPath ?? throw new InvalidOperationException("ProcessPath is null");
}
