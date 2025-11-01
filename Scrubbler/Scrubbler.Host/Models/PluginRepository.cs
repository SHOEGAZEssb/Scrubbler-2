namespace Scrubbler.Host.Models;

/// <summary>
/// Represents a plugin repository from which plugins can be downloaded.
/// </summary>
/// <param name="Name">The display name of the repository.</param>
/// <param name="Url">The URL to the repository's plugin manifest JSON file.</param>
public record PluginRepository(string Name, string Url);
