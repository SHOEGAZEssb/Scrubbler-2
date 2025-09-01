using Microsoft.UI.Xaml.Controls;
using Scrubbler.Abstractions.Logging;
using Scrubbler.Abstractions.Plugin;

namespace Scrubbler.Plugin.ManualScrobbler;

public class ManualScrobblePlugin : IScrobblePlugin
{
    #region Properties

    public string Name => "Manual Scrobbler";

    public string Description => "Enter track details manually and scrobble it";
    public Version Version => typeof(ManualScrobblePlugin).Assembly.GetName().Version!;

    public PlatformSupport SupportedPlatforms => PlatformSupport.All;

    public IconSource? Icon => new SymbolIconSource() { Symbol = Symbol.Manage };

    public ILogService LogService { get; set; }

    private readonly ManualScrobbleViewModel _vm = new();

    #endregion Properties

    public ManualScrobblePlugin()
    {
        LogService = new NoopLogger();
    }

    //public Task<IEnumerable<ScrobbleData>> GetScrobblesAsync(CancellationToken ct)
    //{
    //    return Task.FromResult(_vm.GetScrobbles());
    //}

    public IPluginViewModel GetViewModel()
    {
        return _vm;
    }
}
