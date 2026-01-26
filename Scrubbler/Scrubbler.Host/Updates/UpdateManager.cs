using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

namespace Scrubbler.Host.Updates;

/// <summary>
/// Coordinates update check, download, verification, and handoff to the external updater.
/// </summary>
public sealed class UpdateManager
{
    private readonly HttpClient _http;
    private readonly IUpdateSource _source;
    private readonly UpdateManagerOptions _options;

    /// <summary>
    /// Creates a new instance of <see cref="UpdateManager"/>.
    /// </summary>
    public UpdateManager(HttpClient http, IUpdateSource source, UpdateManagerOptions? options = null)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _options = options ?? new UpdateManagerOptions();
    }

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
        if (update is null)
            throw new ArgumentNullException(nameof(update));

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
/// Configures how <see cref="UpdateManager"/> locates paths and names.
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
        () => typeof(UpdateManager).Assembly.GetName().Version ?? new Version(0, 0, 0);

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

/// <summary>
/// Latest update info resolved for the current runtime.
/// </summary>
public sealed record UpdateInfo(Version Version, Uri PackageUri, string Sha256, string? Notes = null);

/// <summary>
/// Update source abstraction (GitHub Releases, static JSON, local folder, etc.).
/// </summary>
public interface IUpdateSource
{
    /// <summary>
    /// Gets the latest available update for the given runtime identifier, or null if none is available.
    /// </summary>
    Task<UpdateInfo?> GetLatestAsync(Version currentVersion, string rid, CancellationToken ct);
}

/// <summary>
/// Simple JSON-manifest update source (supports http(s) and file URIs).
/// </summary>
internal sealed class JsonManifestUpdateSource : IUpdateSource
{
    private readonly Uri _manifestUri;
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Creates a new instance of <see cref="JsonManifestUpdateSource"/>.
    /// </summary>
    public JsonManifestUpdateSource(Uri manifestUri, HttpClient http)
    {
        _manifestUri = manifestUri ?? throw new ArgumentNullException(nameof(manifestUri));
        _http = http ?? throw new ArgumentNullException(nameof(http));
    }

    /// <inheritdoc />
    public async Task<UpdateInfo?> GetLatestAsync(Version currentVersion, string rid, CancellationToken ct)
    {
        var json = await ReadManifestJsonAsync(ct).ConfigureAwait(false);

        var manifest = JsonSerializer.Deserialize<UpdateManifest>(json, _jsonOptions)
                       ?? throw new InvalidOperationException("invalid manifest json");

        var latest = Version.Parse(manifest.Version);
        if (latest <= currentVersion)
            return null;

        if (!manifest.Artifacts.TryGetValue(rid, out var artifact))
            return null;

        var packageUri = new Uri(_manifestUri, artifact.Url);

        return new UpdateInfo(
            Version: latest,
            PackageUri: packageUri,
            Sha256: artifact.Sha256,
            Notes: manifest.Notes);
    }

    private async Task<string> ReadManifestJsonAsync(CancellationToken ct)
    {
        if (_manifestUri.Scheme.Equals("file", StringComparison.OrdinalIgnoreCase))
            return await File.ReadAllTextAsync(_manifestUri.LocalPath, ct).ConfigureAwait(false);

        return await _http.GetStringAsync(_manifestUri, ct).ConfigureAwait(false);
    }

    private sealed record UpdateManifest(string Version, string? Notes, Dictionary<string, UpdateArtifact> Artifacts);

    private sealed record UpdateArtifact(string Url, string Sha256);
}
