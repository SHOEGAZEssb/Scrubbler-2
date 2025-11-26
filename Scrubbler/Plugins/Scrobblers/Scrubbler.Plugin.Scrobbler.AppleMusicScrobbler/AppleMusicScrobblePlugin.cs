using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Logging;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Plugin.Account;
using Scrubbler.Abstractions.Settings;
using Shoegaze.LastFM;

namespace Scrubbler.Plugin.Scrobbler.AppleMusicScrobbler;

public class AppleMusicScrobblePlugin : IAutoScrobblePlugin, IPersistentPlugin, IAcceptAccountFunctions
{
    #region Properties

    /// <summary>
    /// Gets the display name of the plugin.
    /// </summary>
    public string Name => "Apple Music Scrobbler";

    /// <summary>
    /// Gets a description of what the plugin does.
    /// </summary>
    public string Description => "Scrobble tracks from another last.fm user";

    /// <summary>
    /// Gets the version of the plugin.
    /// </summary>
    public Version Version => typeof(AppleMusicScrobblePlugin).Assembly.GetName().Version!;

    /// <summary>
    /// Gets the platforms this plugin supports.
    /// </summary>
    public PlatformSupport SupportedPlatforms => PlatformSupport.Windows;

    /// <summary>
    /// Gets the icon source for displaying this plugin in the UI.
    /// </summary>
    public IconSource? Icon => new SymbolIconSource() { Symbol = Symbol.MusicInfo };

    /// <summary>
    /// Gets or sets the logging service for this plugin.
    /// </summary>
    /// <seealso cref="ILogService"/>
    public ILogService LogService
    {
        get => _logService;
        set
        {
            _logService = value;
            _vm.Logger = value;
        }
    }
    private ILogService _logService;

    private readonly ApiKeyStorage _apiKeyStorage;
    private readonly AppleMusicScrobbleViewModel _vm;
    private readonly ISettingsStore _settingsStore;
    private PluginSettings _settings = new();

    #endregion Properties

    /// <summary>
    /// Initializes a new instance of the <see cref="ManualScrobblePlugin"/> class.
    /// </summary>
    public AppleMusicScrobblePlugin()
    {
        _logService = new NoopLogger();

        var pluginDir = Path.GetDirectoryName(GetType().Assembly.Location)!;
        _apiKeyStorage = new ApiKeyStorage(PluginDefaults.ApiKey, PluginDefaults.ApiSecret, Path.Combine(pluginDir, "environment.env"));
        _settingsStore = new JsonSettingsStore(Path.Combine(pluginDir, "settings.json"));
        _vm = new AppleMusicScrobbleViewModel(new LastfmClient(_apiKeyStorage.ApiKey, _apiKeyStorage.ApiSecret), LogService);
    }

    /// <summary>
    /// Gets the view model instance for this plugin's UI.
    /// </summary>
    /// <returns>The <see cref="IPluginViewModel"/> instance for this plugin.</returns>
    public IPluginViewModel GetViewModel()
    {
        return _vm;
    }

    public async Task LoadAsync()
    {
        LogService.Debug("Loading settings...");

        _settings = await _settingsStore.GetOrCreateAsync<PluginSettings>(Name);
        _vm.SetInitialAutoConnectState(_settings.AutoConnect);
    }

    public async Task SaveAsync()
    {
        LogService.Debug("Saving settings...");

        _settings.AutoConnect = _vm.AutoConnect;
        await _settingsStore.SetAsync(Name, _settings);
    }

    public void SetAccountFunctionsContainer(AccountFunctionContainer container)
    {
        _vm.FunctionContainer = container;
    }
}
