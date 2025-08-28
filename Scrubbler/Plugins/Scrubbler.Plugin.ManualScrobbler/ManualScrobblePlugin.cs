using Microsoft.UI.Xaml.Controls;
using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Logging;
using Scrubbler.Abstractions.Plugin;

namespace Scrubbler.Plugin.ManualScrobbler;

public class ManualScrobblePlugin : IScrobblePlugin
{
    public string Name => "Manual Scrobbler";

    public string Description => "Allows you to manually enter scrobble data";

    public PlatformSupport SupportedPlatforms => PlatformSupport.All;

    public IconSource? Icon => null;

    public ILogService LogService { get; set; }

    private readonly ManualScrobbleViewModel _vm = new();

    public ManualScrobblePlugin()
    {
        LogService = new NoopLogger();
    }

    public Task<IEnumerable<ScrobbleData>> GetScrobblesAsync(CancellationToken ct)
    {
        return Task.FromResult(_vm.GetScrobbles());
    }

    public IPluginViewModel GetViewModel()
    {
        return _vm;
    }
}
