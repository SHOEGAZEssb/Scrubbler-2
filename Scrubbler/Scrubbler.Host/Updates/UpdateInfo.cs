namespace Scrubbler.Host.Updates;

/// <summary>
/// Latest update info resolved for the current runtime.
/// </summary>
public sealed record UpdateInfo(Version Version, Uri PackageUri, string Sha256, string? Notes = null);
