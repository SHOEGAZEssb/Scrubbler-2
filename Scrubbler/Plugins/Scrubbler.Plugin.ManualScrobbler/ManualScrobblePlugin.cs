using Microsoft.UI.Xaml.Controls;
using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Logging;
using Scrubbler.Abstractions.Plugin;

namespace Scrubbler.Plugin.ManualScrobbler;

public class ManualScrobblePlugin : IScrobblePlugin
{
    #region Properties

    public string Name => Metadata.Name;

    public string Description => Metadata.Description;
    public Version Version => Metadata.Version;

    public PlatformSupport SupportedPlatforms => PlatformSupport.All;

    public IconSource? Icon => new SymbolIconSource() { Symbol = Symbol.Manage };

    public ILogService LogService { get; set; }

    public IPluginMetadata Metadata =>
    new PluginMetadata(
        Id: "scrobbler.manual",
        Name: "Manual Scrobbler",
        typeof(ManualScrobblePlugin).Assembly.GetName().Version!,
        Description: "Enter track details manually and scrobble it",
        SupportedPlatforms: new[] { "All" }
    );

    private readonly ManualScrobbleViewModel _vm = new();

    #endregion Properties

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
