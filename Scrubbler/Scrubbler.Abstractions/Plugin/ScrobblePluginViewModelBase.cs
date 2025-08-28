namespace Scrubbler.Abstractions.Plugin;
public abstract class ScrobblePluginViewModelBase : PluginViewModelBase
{
    public abstract IEnumerable<ScrobbleData> GetScrobbles();
}
