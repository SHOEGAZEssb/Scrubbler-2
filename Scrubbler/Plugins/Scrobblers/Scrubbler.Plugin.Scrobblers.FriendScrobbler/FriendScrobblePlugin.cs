using Microsoft.UI.Xaml.Controls;
using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Logging;
using Scrubbler.Abstractions.Plugin;
using Shoegaze.LastFM;

namespace Scrubbler.Plugin.Scrobblers.FriendScrobbler;

public class FriendScrobblePlugin : IScrobblePlugin
{
    #region Properties

    /// <summary>
    /// Gets the display name of the plugin.
    /// </summary>
    public string Name => "Friend Scrobbler";

    /// <summary>
    /// Gets a description of what the plugin does.
    /// </summary>
    public string Description => "Scrobble tracks from another last.fm user";

    /// <summary>
    /// Gets the version of the plugin.
    /// </summary>
    public Version Version => typeof(FriendScrobblePlugin).Assembly.GetName().Version!;

    /// <summary>
    /// Gets the platforms this plugin supports.
    /// </summary>
    public PlatformSupport SupportedPlatforms => PlatformSupport.All;

    /// <summary>
    /// Gets the icon source for displaying this plugin in the UI.
    /// </summary>
    public IconSource? Icon => new SymbolIconSource() { Symbol = Symbol.OtherUser };

    /// <summary>
    /// Gets or sets the logging service for this plugin.
    /// </summary>
    /// <seealso cref="ILogService"/>
    public ILogService LogService { get; set; }

    private readonly ApiKeyStorage _apiKeyStorage;
    private readonly FriendScrobbleViewModel _vm;

    #endregion Properties

    /// <summary>
    /// Initializes a new instance of the <see cref="ManualScrobblePlugin"/> class.
    /// </summary>
    public FriendScrobblePlugin()
    {
        LogService = new NoopLogger();

        var pluginDir = Path.GetDirectoryName(GetType().Assembly.Location)!;
        _apiKeyStorage = new ApiKeyStorage(PluginDefaults.ApiKey, PluginDefaults.ApiSecret, Path.Combine(pluginDir, "environment.env"));
        _vm = new FriendScrobbleViewModel(new LastfmClient(_apiKeyStorage.ApiKey, _apiKeyStorage.ApiSecret));
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
