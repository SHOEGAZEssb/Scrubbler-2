using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scrubbler.Abstractions;

namespace Scrubbler.Host.Services;

/// <summary>
/// Manages discovery, installation, and lifetime of plugins.
/// </summary>
public interface IPluginManager
{
    /// <summary>
    /// Returns the plugins currently installed and active.
    /// </summary>
    IEnumerable<IPlugin> InstalledPlugins { get; }

    /// <summary>
    /// Returns the plugins available for installation (from repositories).
    /// May be empty until repositories are queried.
    /// </summary>
    IEnumerable<IPluginMetadata> AvailablePlugins { get; }

    /// <summary>
    /// Installs a plugin given its metadata.
    /// </summary>
    Task InstallAsync(IPluginMetadata plugin);

    /// <summary>
    /// Uninstalls a plugin.
    /// </summary>
    Task UninstallAsync(IPlugin plugin);
}
