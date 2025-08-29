using Scrubbler.Abstractions.Plugin;

namespace Scrubbler.Host.Services;

/// <summary>
/// Manages discovery, installation, and lifetime of plugins.
/// </summary>
public interface IPluginManager
{
    /// <summary>
    /// Returns the plugins currently installed and active.
    /// </summary>
    List<IPlugin> InstalledPlugins { get; }

    /// <summary>
    /// Returns the plugins available for installation (from repositories).
    /// May be empty until repositories are queried.
    /// </summary>
    List<PluginManifestEntry> AvailablePlugins { get; }

    /// <summary>
    /// Installs a plugin given its metadata.
    /// </summary>
    Task InstallAsync(PluginManifestEntry plugin);

    /// <summary>
    /// Uninstalls a plugin.
    /// </summary>
    Task UninstallAsync(IPlugin plugin);
}
