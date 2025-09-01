namespace Scrubbler.Abstractions.Plugin;
public interface IScrobblePluginViewModel : IPluginViewModel
{
    Task<IEnumerable<ScrobbleData>> GetScrobblesAsync();

    bool CanScrobble {  get; }
}
