using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Services;

namespace Scrubbler.Plugin.Scrobblers.ManualScrobbler;

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

    private readonly ILogService _logService;
    private readonly ManualScrobbleViewModel _vm = new();

    #endregion Properties

    /// <summary>
    /// Initializes a new instance of the <see cref="ManualScrobblePlugin"/> class.
    /// </summary>
    public ManualScrobblePlugin(IModuleLogServiceFactory logFactory)
    {
        _logService = logFactory.Create(Name);
    }

    /// <summary>
    /// Gets the view model instance for this plugin's UI.
    /// </summary>
    /// <returns>The <see cref="IPluginViewModel"/> instance for this plugin.</returns>
    public IPluginViewModel GetViewModel()
    {
        return _vm;
    }
}
