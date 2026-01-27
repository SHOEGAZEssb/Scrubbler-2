using System.Text.Json;

namespace Scrubbler.Host.Updates;

/// <summary>
/// Simple JSON-manifest update source (supports http(s) and file URIs).
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="JsonManifestUpdateSource"/>.
/// </remarks>
internal sealed class JsonManifestUpdateSource(Uri manifestUri, HttpClient http) : IUpdateSource
{
    private readonly Uri _manifestUri = manifestUri ?? throw new ArgumentNullException(nameof(manifestUri));
    private readonly HttpClient _http = http ?? throw new ArgumentNullException(nameof(http));

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

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
