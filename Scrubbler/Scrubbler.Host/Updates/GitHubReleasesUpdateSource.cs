using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Scrubbler.Host.Updates;

internal sealed class GitHubReleasesUpdateSource : IUpdateSource
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;
    private readonly GitHubReleasesOptions _options;

    /// <summary>
    /// Creates a GitHub releases update source for a given repository.
    /// </summary>
    public GitHubReleasesUpdateSource(HttpClient http, GitHubReleasesOptions options)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _options = options ?? throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(_options.Owner))
            throw new ArgumentException("owner is required", nameof(options));

        if (string.IsNullOrWhiteSpace(_options.Repo))
            throw new ArgumentException("repo is required", nameof(options));

        // configure required github headers once
        _http.DefaultRequestHeaders.Accept.Clear();
        _http.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

        // github requires a user-agent
        if (_http.DefaultRequestHeaders.UserAgent.Count == 0)
            _http.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent);

        // optional: pin an api version header (recommended in docs, but not strictly required)
        if (!string.IsNullOrWhiteSpace(_options.ApiVersion))
            _http.DefaultRequestHeaders.TryAddWithoutValidation("X-GitHub-Api-Version", _options.ApiVersion);

        // optional auth to avoid low unauthenticated rate limit
        if (!string.IsNullOrWhiteSpace(_options.Token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.Token);
    }

    public async Task<UpdateInfo?> GetLatestAsync(Version currentVersion, string rid, CancellationToken ct)
    {
        var url = $"https://api.github.com/repos/{_options.Owner}/{_options.Repo}/releases/latest";

        using var resp = await _http.GetAsync(url, ct).ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode)
            return null; // keep it non-fatal; log if you want

        var json = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        var release = JsonSerializer.Deserialize<GitHubRelease>(json, _jsonOptions)
                      ?? throw new InvalidOperationException("invalid github release json");

        var latestVersion = ParseVersionFromTag(release.TagName);
        if (latestVersion <= currentVersion)
            return null;

        var asset = FindBestAsset(release.Assets, rid, _options.AssetNamePrefix, _options.AssetFileExtension);
        if (asset is null)
            return null;

        var sha256 = TryGetSha256(asset);
        if (string.IsNullOrWhiteSpace(sha256))
            throw new InvalidOperationException("could not resolve sha256 for release asset");

        return new UpdateInfo(latestVersion, new Uri(asset.BrowserDownloadUrl), sha256, release.Body);
    }

    private static Version ParseVersionFromTag(string tag)
    {
        // supports "v1.2.3" and "1.2.3"
        if (tag.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            tag = tag[1..];

        // allow "1.2.3.4" too
        if (!Version.TryParse(tag, out var v))
            throw new InvalidOperationException($"invalid release tag version: '{tag}'");

        return v;
    }

    private static GitHubAsset? FindBestAsset(
        IReadOnlyList<GitHubAsset> assets,
        string rid,
        string assetNamePrefix,
        string assetFileExtension)
    {
        // preferred: exact prefix + rid + extension
        var exactName = $"{assetNamePrefix}-{rid}{assetFileExtension}";
        var exact = assets.FirstOrDefault(a =>
            string.Equals(a.Name, exactName, StringComparison.OrdinalIgnoreCase));

        if (exact is not null)
            return exact;

        // fallback: any asset that contains the rid and ends with the expected extension
        return assets.FirstOrDefault(a =>
            a.Name.Contains(rid, StringComparison.OrdinalIgnoreCase) &&
            a.Name.EndsWith(assetFileExtension, StringComparison.OrdinalIgnoreCase));
    }

    private static string? TryGetSha256(GitHubAsset asset)
    {
        if (string.IsNullOrWhiteSpace(asset.Digest))
            return null;

        const string prefix = "sha256:";
        if (!asset.Digest.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return null;

        return asset.Digest[prefix.Length..];
    }

    private sealed record GitHubRelease(
        [property: JsonPropertyName("tag_name")] string TagName,
        [property: JsonPropertyName("body")] string? Body,
        [property: JsonPropertyName("assets")] List<GitHubAsset> Assets);

    private sealed record GitHubAsset(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("browser_download_url")] string BrowserDownloadUrl,
        [property: JsonPropertyName("digest")] string? Digest);
}

internal sealed class GitHubReleasesOptions
{
    public string Owner { get; init; } = string.Empty;
    public string Repo { get; init; } = string.Empty;

    // optional: token to avoid unauthenticated rate limits
    public string? Token { get; init; }

    // github requires a user-agent
    public string UserAgent { get; init; } = "Scrubbler/2";

    // recommended api version header; can be null
    public string? ApiVersion { get; init; } = "2022-11-28";

    // your asset naming
    public string AssetNamePrefix { get; init; } = "Scrubbler";
    public string AssetFileExtension { get; init; } = ".zip";
}
