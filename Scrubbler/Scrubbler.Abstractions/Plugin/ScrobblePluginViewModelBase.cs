using CommunityToolkit.Mvvm.ComponentModel;

namespace Scrubbler.Abstractions.Plugin;

/// <summary>
/// Base class for view models of plugins that allow manual scrobbling.
/// </summary>
/// <remarks>
/// Inherit from this class when creating a plugin that lets users manually select or enter tracks to scrobble.
/// </remarks>
/// <seealso cref="PluginViewModelBase"/>
/// <seealso cref="IScrobblePluginViewModel"/>
public abstract partial class ScrobblePluginViewModelBase : PluginViewModelBase, IScrobblePluginViewModel
{
    /// <summary>
    /// Gets a value indicating whether scrobbling can be performed.
    /// </summary>
    /// <returns><c>true</c> if the plugin is ready to scrobble (e.g., tracks are selected and account is authenticated); otherwise, <c>false</c>.</returns>
    public abstract bool CanScrobble { get; }

    /// <summary>
    /// Gets the collection of scrobbles that the user has selected or entered.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the collection of <see cref="ScrobbleData"/> to be scrobbled.</returns>
    public abstract Task<IEnumerable<ScrobbleData>> GetScrobblesAsync();
}
