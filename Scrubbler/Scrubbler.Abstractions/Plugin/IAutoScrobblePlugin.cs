namespace Scrubbler.Abstractions.Plugin;
public interface IAutoScrobblePlugin : IPlugin
{
    event EventHandler<IEnumerable<ScrobbleData>> ScrobblesDetected;
}
