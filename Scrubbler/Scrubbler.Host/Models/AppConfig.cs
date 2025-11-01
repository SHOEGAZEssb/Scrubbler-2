namespace Scrubbler.Host.Models;

/// <summary>
/// Represents application configuration settings.
/// </summary>
public record AppConfig
{
    /// <summary>
    /// Gets the environment name (e.g., "Development", "Production").
    /// </summary>
    public string? Environment { get; init; }
}
