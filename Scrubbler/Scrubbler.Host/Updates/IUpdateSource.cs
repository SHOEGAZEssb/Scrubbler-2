namespace Scrubbler.Host.Updates;

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
