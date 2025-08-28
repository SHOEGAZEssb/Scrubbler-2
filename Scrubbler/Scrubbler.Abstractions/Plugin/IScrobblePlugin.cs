namespace Scrubbler.Abstractions.Plugin;
public interface IScrobblePlugin : IPlugin
{
    Task<IEnumerable<ScrobbleData>> GetScrobblesAsync(CancellationToken ct);
}
