namespace Scrubbler.Abstractions.Plugin;

/// <summary>
/// Interface for view models of plugins that allow users to manually select tracks to scrobble.
/// </summary>
/// <seealso cref="IPluginViewModel"/>
/// <seealso cref="IScrobblePlugin"/>
/// <seealso cref="ScrobblePluginViewModelBase"/>
public interface IScrobblePluginViewModel : IPluginViewModel
{
    /// <summary>
    /// Gets the collection of scrobbles that the user has selected or entered.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the collection of <see cref="ScrobbleData"/> to be scrobbled.</returns>
    Task<IEnumerable<ScrobbleData>> GetScrobblesAsync();

    /// <summary>
    /// Gets a value indicating whether scrobbling can be performed (e.g., tracks are selected and account is authenticated).
    /// </summary>
    bool CanScrobble {  get; }
}
