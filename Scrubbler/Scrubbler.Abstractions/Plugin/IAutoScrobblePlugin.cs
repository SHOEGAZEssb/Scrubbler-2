namespace Scrubbler.Abstractions.Plugin;

/// <summary>
/// Marker interface for plugins that automatically detect and scrobble tracks without user intervention.
/// </summary>
/// <seealso cref="IPlugin"/>
/// <seealso cref="IAutoScrobblePluginViewModel"/>
public interface IAutoScrobblePlugin : IPlugin
{
}
