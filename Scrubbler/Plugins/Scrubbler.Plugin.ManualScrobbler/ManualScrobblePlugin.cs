using Scrubbler.Abstractions;

namespace Scrubbler.Plugin.ManualScrobbler;

public class ManualScrobblePlugin : IScrobblePlugin
{
    public string Name => "Manual Scrobbler";

    public string Description => "Allows you to manually enter scrobble data";

    public PlatformSupport SupportedPlatforms => PlatformSupport.All;

    private readonly ManualScrobbleViewModel _vm = new();

    public Task<IEnumerable<ScrobbleData>> GetScrobblesAsync(CancellationToken ct)
    {
        return Task.FromResult(_vm.GetScrobbles());
    }

    public IPluginViewModel GetViewModel()
    {
        return _vm;
    }
}
