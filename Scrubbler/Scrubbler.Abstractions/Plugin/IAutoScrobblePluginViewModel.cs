namespace Scrubbler.Abstractions.Plugin;

/// <summary>
/// Interface for view models of plugins that automatically detect tracks and notify when scrobbles are available.
/// </summary>
/// <seealso cref="IPluginViewModel"/>
/// <seealso cref="IAutoScrobblePlugin"/>
public interface IAutoScrobblePluginViewModel : IPluginViewModel
{
    /// <summary>
    /// Event that is raised when the plugin detects new scrobbles that should be submitted.
    /// </summary>
    /// <remarks>
    /// Handlers of this event should scrobble the tracks using an <see cref="IAccountPlugin"/>.
    /// </remarks>
    event EventHandler<IEnumerable<ScrobbleData>>? ScrobblesDetected;
}
