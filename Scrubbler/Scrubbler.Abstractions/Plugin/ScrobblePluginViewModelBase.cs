using CommunityToolkit.Mvvm.ComponentModel;

namespace Scrubbler.Abstractions.Plugin;

public abstract partial class ScrobblePluginViewModelBase : PluginViewModelBase, IScrobblePluginViewModel
{
    public abstract bool CanScrobble { get; }

    public abstract Task<IEnumerable<ScrobbleData>> GetScrobblesAsync();
}
