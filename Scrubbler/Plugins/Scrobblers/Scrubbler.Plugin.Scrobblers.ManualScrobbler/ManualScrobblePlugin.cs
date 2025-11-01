using Microsoft.UI.Xaml.Controls;
using Scrubbler.Abstractions.Logging;
using Scrubbler.Abstractions.Plugin;

namespace Scrubbler.Plugin.ManualScrobbler;

/// <summary>
/// Plugin that allows users to manually enter track details and scrobble them.
/// </summary>
/// <seealso cref="IScrobblePlugin"/>
public class ManualScrobblePlugin : IScrobblePlugin
{
    #region Properties

    /// <summary>
    /// Gets the display name of the plugin.
    /// </summary>
    public string Name => "Manual Scrobbler";

    /// <summary>
    /// Gets a description of what the plugin does.
    /// </summary>
    public string Description => "Enter track details manually and scrobble it";
    
    /// <summary>
    /// Gets the version of the plugin.
    /// </summary>
    public Version Version => typeof(ManualScrobblePlugin).Assembly.GetName().Version!;

    /// <summary>
    /// Gets the platforms this plugin supports.
    /// </summary>
    public PlatformSupport SupportedPlatforms => PlatformSupport.All;

    /// <summary>
    /// Gets the icon source for displaying this plugin in the UI.
    /// </summary>
    public IconSource? Icon => new SymbolIconSource() { Symbol = Symbol.Manage };

    /// <summary>
    /// Gets or sets the logging service for this plugin.
    /// </summary>
    /// <seealso cref="ILogService"/>
    public ILogService LogService { get; set; }

    private readonly ManualScrobbleViewModel _vm = new();

    #endregion Properties

    /// <summary>
    /// Initializes a new instance of the <see cref="ManualScrobblePlugin"/> class.
    /// </summary>
    public ManualScrobblePlugin()
    {
        LogService = new NoopLogger();
    }

    //public Task<IEnumerable<ScrobbleData>> GetScrobblesAsync(CancellationToken ct)
    //{
    //    return Task.FromResult(_vm.GetScrobbles());
    //}

    /// <summary>
    /// Gets the view model instance for this plugin's UI.
    /// </summary>
    /// <returns>The <see cref="IPluginViewModel"/> instance for this plugin.</returns>
    public IPluginViewModel GetViewModel()
    {
        return _vm;
    }
}
