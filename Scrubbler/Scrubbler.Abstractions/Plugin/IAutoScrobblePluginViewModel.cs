namespace Scrubbler.Abstractions.Plugin;
public interface IAutoScrobblePluginViewModel : IPluginViewModel
{
    event EventHandler<IEnumerable<ScrobbleData>> ScrobblesDetected;
}
