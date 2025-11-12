using Microsoft.UI.Xaml.Controls;
using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Logging;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Settings;
using Shoegaze.LastFM;
using Shoegaze.LastFM.Authentication;

namespace Scrubbler.Plugin.Accounts.LastFm;

/// <summary>
/// A plugin that connects to a Last.fm account using session keys.
/// Implements IAccountPlugin so authentication persists between runs.
/// </summary>
public class LastFmAccountPlugin : IAccountPlugin
{
    #region Properties

    #region IPlugin

    public string Name => "Last.fm";

    public string Description => "Scrobble to a last.fm account";
    public Version Version => typeof(LastFmAccountPlugin).Assembly.GetName().Version!;

    public PlatformSupport SupportedPlatforms => PlatformSupport.All;

    public IconSource? Icon => new SymbolIconSource() { Symbol = Symbol.Play };

    public ILogService LogService { get; set; }

    private readonly ISecureStore _secureStore;
    private readonly ISettingsStore _settingsStore;
    private PluginSettings _settings = new();

    #endregion IPlugin

    /// <summary>
    /// Gets a value indicating whether the user is currently authenticated with Last.fm.
    /// </summary>
    public bool IsAuthenticated => !string.IsNullOrEmpty(_sessionKey);

    /// <summary>
    /// Gets or sets whether scrobbling to Last.fm is currently enabled.
    /// </summary>
    public bool IsScrobblingEnabled
    {
        get => _settings.IsScrobblingEnabled;
        set 
        {
            if (IsScrobblingEnabled != value)
            {
                _settings.IsScrobblingEnabled = value;
                IsScrobblingEnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Event that is fired when <see cref="IsScrobblingEnabled"/> changes.
    /// </summary>
    public event EventHandler? IsScrobblingEnabledChanged;

    private const string AccountIdKey = "LastFmAccountId";
    private const string SessionKeyKey = "LastFmSessionKey";

    private string? _sessionKey;

    /// <summary>
    /// Gets the Last.fm username of the authenticated account, or <c>null</c> if not authenticated.
    /// </summary>
    public string? AccountId { get; private set; }

    private readonly ApiKeyStorage _apiKeyStorage;
    private ILastfmClient? _lastfmClient;

    #endregion Properties

    /// <summary>
    /// Initializes a new instance of the <see cref="LastFmAccountPlugin"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes storage services for secure data (session keys) and settings.
    /// </remarks>
    public LastFmAccountPlugin()
    {
        LogService = new NoopLogger();

        var pluginDir = Path.GetDirectoryName(GetType().Assembly.Location)!;
        _apiKeyStorage = new ApiKeyStorage(PluginDefaults.ApiKey, PluginDefaults.ApiSecret, Path.Combine(pluginDir, "environment.env"));
        _secureStore = new FileSecureStore(Path.Combine(pluginDir, "settings.dat"), Name);
        _settingsStore = new JsonSettingsStore(Path.Combine(pluginDir, "settings.json"));
    }

    /// <summary>
    /// Loads plugin state from secure or non-secure storage.
    /// Called once at startup.
    /// </summary>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    public async Task LoadAsync()
    {
        LogService.Debug("Loading settings...");

        AccountId = await _secureStore.GetAsync(AccountIdKey);
        _sessionKey = await _secureStore.GetAsync(SessionKeyKey);
        _settings = await _settingsStore.GetOrCreateAsync<PluginSettings>(Name);

        if (!string.IsNullOrEmpty(_sessionKey))
        {
            _lastfmClient = new LastfmClient(_apiKeyStorage.ApiKey, _apiKeyStorage.ApiSecret);
            _lastfmClient.SetSessionKey(_sessionKey);
        }
    }

    /// <summary>
    /// Saves plugin state to secure or non-secure storage.
    /// Called when application exits or when plugin requests persistence.
    /// </summary>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    public async Task SaveAsync()
    {
        if (AccountId == null)
            await _secureStore.RemoveAsync(AccountIdKey);
        else
            await _secureStore.SaveAsync(AccountIdKey, AccountId);

        if (_sessionKey == null)
            await _secureStore.RemoveAsync(SessionKeyKey);
        else
            await _secureStore.SaveAsync(SessionKeyKey, _sessionKey);

        await _settingsStore.SetAsync(Name, _settings);
    }

    /// <summary>
    /// Initiates an OAuth authentication flow with Last.fm.
    /// </summary>
    /// <returns>A task that represents the asynchronous authentication operation.</returns>
    /// <exception cref="Exception">Thrown when OAuth authentication fails.</exception>
    public async Task AuthenticateAsync()
    {
        if (_apiKeyStorage.ApiKey == null || _apiKeyStorage.ApiSecret == null)
        {
            // todo: throw, log ?
            return;
        }

        LogService.Debug("Starting OAuth flow");
        try
        {
            var a = new LastfmAuthService(_apiKeyStorage.ApiKey, _apiKeyStorage.ApiSecret);
            var session = await a.AuthenticateAsync();

            AccountId = session.Username;
            _sessionKey = session.SessionKey;

            _lastfmClient = new LastfmClient(_apiKeyStorage.ApiKey, _apiKeyStorage.ApiSecret);
            _lastfmClient.SetSessionKey(_sessionKey);
            LogService.Debug($"Finished OAuth flow. Logged in as {AccountId}");
        }
        catch (Exception ex)
        {
            LogService.Error("Error during OAuth flow.", ex);
        }
    }

    /// <summary>
    /// Logs out the Last.fm account and clears authentication state.
    /// </summary>
    /// <returns>A task that represents the asynchronous logout operation.</returns>
    public Task LogoutAsync()
    {
        AccountId = null;
        _sessionKey = null;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the view model instance for this plugin's UI.
    /// </summary>
    /// <returns>A new instance of <see cref="IPluginViewModel"/> for this plugin.</returns>
    /// <exception cref="NotImplementedException">This method is not yet implemented.</exception>
    public IPluginViewModel GetViewModel()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Submits the provided scrobbles to Last.fm.
    /// </summary>
    /// <param name="scrobbles">The collection of tracks to scrobble.</param>
    /// <returns>A task that represents the asynchronous scrobble operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the account is not authenticated or scrobbling is disabled.</exception>
    public async Task ScrobbleAsync(IEnumerable<ScrobbleData> scrobbles)
    {
        if (!IsScrobblingEnabled || !IsAuthenticated)
        {
            LogService.Warn("Tried to scrobble, but scrobbling was not enabled, or client was not authenticated");
            return;
        }

        var s = scrobbles.Select(s => new Shoegaze.LastFM.Track.ScrobbleData(s.Artist, s.Track, s.Timestamp, s.Album, s.AlbumArtist));
        int batches = (int)Math.Ceiling(s.Count() / 50d);
        int i = 1;
        foreach (var batch in s.Chunk(50))
        {
            LogService.Info($"Scrobbling batch {i++} / {batches}...");
            var response = await _lastfmClient!.Track.ScrobbleAsync(batch);
            LogService.Info($"Scrobble Status: {response.LastFmStatus}");
        }
    }
}
