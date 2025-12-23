using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Plugin;

namespace Scrubbler.Plugin.Scrobbler.Mock;

internal class TestViewModel : ScrobblePluginViewModelBase
{
    public override bool CanScrobble => true;

    public override async Task<IEnumerable<ScrobbleData>> GetScrobblesAsync()
    {
        await Task.Delay(0);
        return [];
    }
}
