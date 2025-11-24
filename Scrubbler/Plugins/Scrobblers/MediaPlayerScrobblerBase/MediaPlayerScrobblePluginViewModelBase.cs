using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Logging;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Plugin.Account;
using Shoegaze.LastFM;

namespace Scrubbler.Plugins.Scrobblers.MediaPlayerScrobbleBase;

public abstract partial class MediaPlayerScrobblePluginViewModelBase(ILastfmClient lastfmClient, ILogService logger) : PluginViewModelBase, IAutoScrobblePluginViewModel
{
    #region Properties

    public event EventHandler<IEnumerable<ScrobbleData>>? ScrobblesDetected;

    [ObservableProperty]
    protected bool _isConnected;

    [ObservableProperty]
    private bool _autoConnect;

    public ILogService Logger { get; set; } = logger;

    public ICanLoveTracks? LoveTrackObject { get; set; }

    public bool CanLoveTracks => LoveTrackObject != null;

    protected readonly ILastfmClient _lastfmClient = lastfmClient;

    #region Track Properties

    /// <summary>
    /// The name of the current playing track.
    /// </summary>
    public abstract string CurrentTrackName { get; }

    /// <summary>
    /// The name of the current artist.
    /// </summary>
    public abstract string CurrentArtistName { get; }

    /// <summary>
    /// The name of the current album.
    /// </summary>
    public abstract string CurrentAlbumName { get; }

    /// <summary>
    /// The length of the current track.
    /// </summary>
    public abstract int CurrentTrackLength { get; }

    /// <summary>
    /// Seconds needed to listen to the current song to scrobble it.
    /// (Max <see cref="MAXSECONDSTOSCROBBLE"/>)
    /// </summary>
    public int CurrentTrackLengthToScrobble
    {
        get
        {
            int sec = (int)Math.Ceiling(CurrentTrackLength * PercentageToScrobble);
            return sec < MAXSECONDSTOSCROBBLE ? sec :MAXSECONDSTOSCROBBLE;
        }
    }

    [ObservableProperty]
    protected bool _currentTrackScrobbled;

    //public ObservableCollection<TagViewModel> CurrentTrackTags { get; } // todo

    [ObservableProperty]
    protected Uri? _currentAlbumArtwork;

    [ObservableProperty]
    protected int _currentTrackPlayCount;

    [ObservableProperty]
    protected int _currentArtistPlayCount;

    [ObservableProperty]
    protected int _currentAlbumPlayCount;

    [ObservableProperty]
    private bool _currentTrackLoved;

    #endregion Track Properties

    [ObservableProperty]
    protected int _countedSeconds;

    [ObservableProperty]
    private double _percentageToScrobble = 0.5d;

    /// <summary>
    /// Maximum seconds it should take to scrobble a track.
    /// </summary>
    private const int MAXSECONDSTOSCROBBLE = 240;

    #endregion Properties

    [RelayCommand]
    private async Task ToggleConnection()
    {
        if (IsConnected)
            await Disconnect();
        else
            await Connect();
    }

    public void SetInitialAutoConnectState(bool autoConnect)
    {
        AutoConnect = autoConnect;

        if (AutoConnect)
        {
            Logger.Info("Auto-connect is enabled. Attempting to connect...");
            _ = Connect();
        }
    }

    /// <summary>
    /// Connects to the client.
    /// </summary>
    protected abstract Task Connect();

    /// <summary>
    /// Disconnects from the client.
    /// </summary>
    protected abstract Task Disconnect();

    /// <summary>
    /// Notifies the ui of changed song info.
    /// </summary>
    protected virtual void UpdateCurrentTrackInfo()
    {
        OnPropertyChanged(nameof(CurrentTrackName));
        OnPropertyChanged(nameof(CurrentArtistName));
        OnPropertyChanged(nameof(CurrentAlbumName));
        OnPropertyChanged(nameof(CurrentTrackLength));
        OnPropertyChanged(nameof(CurrentTrackLengthToScrobble));
        //UpdateLovedInfo().Forget();
        //UpdateNowPlaying().Forget();
        //UpdatePlayCountsAndTags().Forget();
        _ = FetchAlbumArtwork();
    }

    protected async Task UpdateLovedInfo()
    {
        if (!CanLoveTracks)
            return;
        try
        {
            var errorMessage = await LoveTrackObject.GetLoveState(CurrentArtistName, CurrentTrackName, CurrentAlbumName, out bool isLoved);
            Logger.Debug($"Updated loved info: {CurrentTrackLoved}");
        }
        catch (Exception ex)
        {
            Logger.Error("Error updating loved info.", ex);
        }
    }

    protected async Task FetchAlbumArtwork()
    {
        if (string.IsNullOrEmpty(CurrentAlbumName) || string.IsNullOrEmpty(CurrentArtistName))
        {
            Logger.Debug("Cannot fetch album artwork: Album name or artist name is empty.");
            CurrentAlbumArtwork = null;
            return;
        }

        var response = await _lastfmClient.Album.GetInfoByNameAsync(CurrentAlbumName, CurrentArtistName);
        if (response.IsSuccess && response.Data != null)
        {
            CurrentAlbumArtwork = response.Data.Images.Values.LastOrDefault();
            Logger.Debug("Fetched album artwork successfully.");
        }
        else
        {
            CurrentAlbumArtwork = null;
            Logger.Debug($"Failed to fetch album artwork: {response.ErrorMessage}");
        }
    }

    protected void OnScrobblesDetected(IEnumerable<ScrobbleData> scrobbles)
    {
        Logger.Info($"Detected {scrobbles.Count()} scrobble(s).");
        ScrobblesDetected?.Invoke(this, scrobbles);
    }
}
