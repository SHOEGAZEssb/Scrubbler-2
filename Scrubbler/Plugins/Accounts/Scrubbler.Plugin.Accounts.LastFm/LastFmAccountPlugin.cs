using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Plugin.Account;
using Scrubbler.Abstractions.Services;
using Scrubbler.Abstractions.Settings;
using Shoegaze.LastFM;
using Shoegaze.LastFM.Authentication;

namespace Scrubbler.Plugin.Accounts.LastFm;

/// <summary>
/// A plugin that connects to a Last.fm account using session keys.
/// Implements IAccountPlugin so authentication persists between runs.
/// </summary>
[PluginMetadata(
    Name = "Last.fm",
    Description = "Scrobble to a Last.fm account",
    SupportedPlatforms = PlatformSupport.All)]
public class LastFmAccountPlugin : PluginBase, IAccountPlugin, IHaveScrobbleLimit, ICanLoveTracks, ICanFetchPlayCounts, ICanFetchTags, ICanUpdateNowPlaying, ICanOpenLinks
{
    #region Properties

    public string? Username => _secureStore.GetAsync(AccountIdKey).GetAwaiter().GetResult() ?? null;

    private readonly ISecureStore _secureStore;
    private readonly ISettingsStore _settingsStore;
    private PluginSettings _settings = new();

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

    public event EventHandler? CurrentScrobbleCountChanged;

    public int ScrobbleLimit => 3000;

    public int CurrentScrobbleCount
    {
        get => _currentScrobbleCount;
        private set
        {
            if (CurrentScrobbleCount != value)
            {
                _currentScrobbleCount = value;
                CurrentScrobbleCountChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    private int _currentScrobbleCount;

    public bool HasReachedScrobbleLimit => CurrentScrobbleCount >= ScrobbleLimit;


    private readonly ApiKeyStorage _apiKeyStorage;
    private ILastfmClient? _lastfmClient;
    private readonly ILinkOpenerService _linkOpener;
    private const string LASTFMMUSICBASEURL = "https://www.last.fm/music/";
    private const string LASTFMTAGBASEURL = "https://www.last.fm/tag/";

    #endregion Properties

    /// <summary>
    /// Initializes a new instance of the <see cref="LastFmAccountPlugin"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes storage services for secure data (session keys) and settings.
    /// </remarks>
    public LastFmAccountPlugin(ILinkOpenerService linkOpener, IModuleLogServiceFactory logFactory)
        : base(logFactory)
    {
        var pluginDir = Path.GetDirectoryName(GetType().Assembly.Location)!;
        _apiKeyStorage = new ApiKeyStorage(PluginDefaults.ApiKey, PluginDefaults.ApiSecret, Path.Combine(pluginDir, "environment.env"));
        _secureStore = new FileSecureStore(Path.Combine(pluginDir, "settings.dat"), Name);
        _settingsStore = new JsonSettingsStore(Path.Combine(pluginDir, "settings.json"));
        _linkOpener = linkOpener;
    }

    /// <summary>
    /// Loads plugin state from secure or non-secure storage.
    /// Called once at startup.
    /// </summary>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    public async Task LoadAsync()
    {
        _logService.Debug("Loading settings...");

        AccountId = await _secureStore.GetAsync(AccountIdKey);
        _sessionKey = await _secureStore.GetAsync(SessionKeyKey);
        _settings = await _settingsStore.GetOrCreateAsync<PluginSettings>(Name);

        if (!string.IsNullOrEmpty(_sessionKey))
        {
            _lastfmClient = new LastfmClient(_apiKeyStorage.ApiKey, _apiKeyStorage.ApiSecret);
            _lastfmClient.SetSessionKey(_sessionKey);
        }

        await UpdateScrobbleCount();
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

        _logService.Debug("Starting OAuth flow");
        try
        {
            var a = new LastfmAuthService(_apiKeyStorage.ApiKey, _apiKeyStorage.ApiSecret);
            var session = await a.AuthenticateAsync();

            AccountId = session.Username;
            _sessionKey = session.SessionKey;

            _lastfmClient = new LastfmClient(_apiKeyStorage.ApiKey, _apiKeyStorage.ApiSecret);
            _lastfmClient.SetSessionKey(_sessionKey);
            _logService.Debug($"Finished OAuth flow. Logged in as {AccountId}");
        }
        catch (Exception ex)
        {
            _logService.Error("Error during OAuth flow.", ex);
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
    public override IPluginViewModel GetViewModel()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Submits the provided scrobbles to Last.fm.
    /// </summary>
    /// <param name="scrobbles">The collection of tracks to scrobble.</param>
    /// <returns>A task that represents the asynchronous scrobble operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the account is not authenticated or scrobbling is disabled.</exception>
    public async Task<ScrobbleResponse> ScrobbleAsync(IEnumerable<ScrobbleData> scrobbles)
    {
        if (!IsScrobblingEnabled || !IsAuthenticated)
        {
            _logService.Warn("Tried to scrobble, but scrobbling was not enabled, or client was not authenticated");
            return new ScrobbleResponse(false, "Not authenticated");
        }

        await UpdateScrobbleCount();
        if (HasReachedScrobbleLimit)
        {
            _logService.Warn("Scrobble limit reached; not scrobbling.");
            return new ScrobbleResponse(false, "Scrobble limit reached");
        }
        else if (CurrentScrobbleCount + scrobbles.Count() > ScrobbleLimit)
        {
            _logService.Warn("Scrobble limit will be exceeded; not scrobbling.");
            return new ScrobbleResponse(false, "Scrobble limit will be exceeded");
        }

        var s = scrobbles.Select(s => new Shoegaze.LastFM.Track.ScrobbleData(s.Artist, s.Track, s.Timestamp, s.Album, s.AlbumArtist));
        int batches = (int)Math.Ceiling(s.Count() / 50d);
        int i = 1;
        foreach (var batch in s.Chunk(50))
        {
            _logService.Info($"Scrobbling batch {i++} / {batches}...");
            var response = await _lastfmClient!.Track.ScrobbleAsync(batch);
            _logService.Info($"Scrobble Status: {response.LastFmStatus}");
            if (!response.IsSuccess)
            {
                _logService.Error("Error during scrobble: " + (response.ErrorMessage ?? "Unknown error"));
                return new ScrobbleResponse(false, response.ErrorMessage ?? "Unknown error");
            }
        }

        await UpdateScrobbleCount();
        return new ScrobbleResponse(true, null);
    }

    public async Task UpdateScrobbleCount()
    {
        _logService.Debug("Updating scrobble count...");

        if (!IsAuthenticated || Username == null || _lastfmClient == null)
        {
            CurrentScrobbleCount = 0;
            _logService.Warn("Cannot update scrobble count: not authenticated");
            return;
        }

        var response = await _lastfmClient.User.GetRecentTracksAsync(Username, extended: false, fromDate: DateTime.Now.Subtract(TimeSpan.FromHours(24)),
                                                                                                toDate: DateTime.Now, ignoreNowPlaying: true);

        if (response.IsSuccess && response.Data != null)
            CurrentScrobbleCount = response.Data.TotalItems;
        else
        {
            CurrentScrobbleCount = 0;
            _logService.Error("Failed to update scrobble count: " + (response.ErrorMessage ?? "Unknown error"));
        }
    }

    #region IAccountFunctions

    public async Task<string?> SetLoveState(string artistName, string trackName, string? albumName, bool isLoved)
    {
        var response = await _lastfmClient!.Track.SetLoveState(artistName, trackName, isLoved);
        return response.IsSuccess ? null : (response.ErrorMessage ?? "Unknown error");
    }

    public async Task<(string? errorMessage, bool isLoved)> GetLoveState(string artistName, string trackName, string? albumName)
    {
        if (Username == null || !IsAuthenticated)
            return ("Not authenticated", false);

        var response = await _lastfmClient!.Track.GetInfoByNameAsync(artistName, trackName, Username);
        if (response.IsSuccess && response.Data != null)
            return (null, response.Data.UserLoved ?? false);
        else
            return (response.ErrorMessage ?? "Unknown error", false);
    }

    public async Task<(string? errorMessage, int playCount)> GetArtistPlayCount(string artistName)
    {
        if (Username == null || !IsAuthenticated)
            return ("Not authenticated", 0);

        var response = await _lastfmClient!.Artist.GetInfoByNameAsync(artistName, Username);
        if (response.IsSuccess && response.Data != null)
            return (null, response.Data.UserPlayCount ?? 0);
        else
            return (response.ErrorMessage ?? "Unknown error", 0);
    }

    public async Task<(string? errorMessage, int playCount)> GetTrackPlayCount(string artistName, string trackName)
    {
        if (Username == null || !IsAuthenticated)
            return ("Not authenticated", 0);

        var response = await _lastfmClient!.Track.GetInfoByNameAsync(trackName, artistName, Username);
        if (response.IsSuccess && response.Data != null)
            return (null, response.Data.UserPlayCount ?? 0);
        else
            return (response.ErrorMessage ?? "Unknown error", 0);
    }

    public async Task<(string? errorMessage, int playCount)> GetAlbumPlayCount(string artistName, string albumName)
    {
        if (Username == null || !IsAuthenticated)
            return ("Not authenticated", 0);

        var response = await _lastfmClient!.Album.GetInfoByNameAsync(albumName, artistName, Username);
        if (response.IsSuccess && response.Data != null)
            return (null, response.Data.UserPlayCount ?? 0);
        else
            return (response.ErrorMessage ?? "Unknown error", 0);
    }

    public async Task<(string? errorMessage, IEnumerable<string> tags)> GetArtistTags(string artistName)
    {
        var response = await _lastfmClient!.Artist.GetTopTagsByNameAsync(artistName);
        if (response.IsSuccess && response.Data != null)
        {
            var tags = response.Data.Select(t => t.Name);
            return (null, tags);
        }
        else
            return (response.ErrorMessage ?? "Unknown error", []);
    }

    public async Task<(string? errorMessage, IEnumerable<string> tags)> GetTrackTags(string artistName, string trackName)
    {
        var response = await _lastfmClient!.Track.GetTopTagsByNameAsync(artistName, trackName);
        if (response.IsSuccess && response.Data != null)
        {
            var tags = response.Data.Select(t => t.Name);
            return (null, tags);
        }
        else
            return (response.ErrorMessage ?? "Unknown error", []);
    }

    public async Task<(string? errorMessage, IEnumerable<string> tags)> GetAlbumTags(string artistName, string albumName)
    {
        var response = await _lastfmClient!.Album.GetTopTagsByNameAsync(artistName, albumName);
        if (response.IsSuccess && response.Data != null)
        {
            var tags = response.Data.Select(t => t.Name);
            return (null, tags);
        }
        else
            return (response.ErrorMessage ?? "Unknown error", []);
    }

    public async Task<string?> UpdateNowPlaying(string artistName, string trackName, string? albumName)
    {
        var response = await _lastfmClient!.Track.UpdateNowPlayingAsync(artistName, trackName, albumName);
        return response.IsSuccess ? null : (response.ErrorMessage ?? "Unknown error");
    }

    public async Task OpenArtistLink(string artistName)
    {
        await _linkOpener.OpenLink($"LASTFMBASEURL{Uri.EscapeDataString(artistName)}");
    }

    public async Task OpenAlbumLink(string albumName, string artistName)
    {
        await _linkOpener.OpenLink($"LASTFMBASEURL{Uri.EscapeDataString(artistName)}/{Uri.EscapeDataString(albumName)}");
    }

    public async Task OpenTrackLink(string trackName, string artistName, string? albumName)
    {
        string album = string.IsNullOrEmpty(albumName) ? "_" : albumName;
        await _linkOpener.OpenLink($"LASTFMBASEURL{Uri.EscapeDataString(artistName)}/{Uri.EscapeDataString(album)}/{Uri.EscapeDataString(trackName)}");
    }

    public async Task OpenTagLink(string tagName)
    {
        await _linkOpener.OpenLink($"LASTFMBASEURL{Uri.EscapeDataString(tagName)}");
    }

    #endregion IAccountFunctions
}
