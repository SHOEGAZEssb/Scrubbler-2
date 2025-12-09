using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Plugin.Account;
using Scrubbler.Abstractions.Services;
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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanLoveTracks))]
    [NotifyPropertyChangedFor(nameof(CanFetchPlayCounts))]
    [NotifyPropertyChangedFor(nameof(CanFetchTags))]
    private AccountFunctionContainer? _functionContainer;

    private bool CanLoveTracks => FunctionContainer?.LoveTrackObject != null;

    public bool CanFetchPlayCounts => FunctionContainer?.FetchPlayCountsObject != null;

    public bool CanFetchTags => FunctionContainer?.FetchTagsObject != null;

    public ICanUpdateNowPlaying? UpdateNowPlayingObject { get; set; }

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
            return sec < MAXSECONDSTOSCROBBLE ? sec : MAXSECONDSTOSCROBBLE;
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
        _ = UpdateNowPlaying();
        _ = UpdatePlayCounts();
        _ = UpdateLovedInfo();
        _ = FetchAlbumArtwork();
    }

    protected void ClearState()
    {
        CurrentTrackPlayCount = -1;
        CurrentArtistPlayCount = -1;
        CurrentAlbumPlayCount = -1;
        CurrentTrackLoved = false;
        CurrentAlbumArtwork = null;
        CountedSeconds = 0;
        CurrentTrackScrobbled = false;
        UpdateCurrentTrackInfo();
    }

    private async Task UpdateNowPlaying()
    {
        if (UpdateNowPlayingObject == null || string.IsNullOrEmpty(CurrentTrackName) || string.IsNullOrEmpty(CurrentArtistName) || string.IsNullOrEmpty(CurrentAlbumName))
            return;

        try
        {
            Logger.Debug("Updating Now Playing...");
            var errorMessage = await UpdateNowPlayingObject.UpdateNowPlaying(CurrentArtistName, CurrentTrackName, CurrentAlbumName);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Logger.Error($"Error updating Now Playing: {errorMessage}");
                return;
            }
            Logger.Debug("Now Playing updated successfully.");
        }
        catch (Exception ex)
        {
            Logger.Error("Error updating Now Playing.", ex);
        }
    }

    private async Task UpdatePlayCounts()
    {
        if (!CanFetchPlayCounts || string.IsNullOrEmpty(CurrentTrackName) || string.IsNullOrEmpty(CurrentArtistName) || string.IsNullOrEmpty(CurrentAlbumName))
            return;

        try
        {
            Logger.Debug("Updating play counts...");
            var (artistError, artistPlayCount) = await FunctionContainer!.FetchPlayCountsObject!.GetArtistPlayCount(CurrentArtistName);
            if (!string.IsNullOrEmpty(artistError))
            {
                Logger.Error($"Error fetching artist play count: {artistError}");
            }
            else
            {
                CurrentArtistPlayCount = artistPlayCount;
                Logger.Debug($"Updated artist play count: {CurrentArtistPlayCount}");
            }
            var (trackError, trackPlayCount) = await FunctionContainer!.FetchPlayCountsObject.GetTrackPlayCount(CurrentArtistName, CurrentTrackName);
            if (!string.IsNullOrEmpty(trackError))
            {
                Logger.Error($"Error fetching track play count: {trackError}");
            }
            else
            {
                CurrentTrackPlayCount = trackPlayCount;
                Logger.Debug($"Updated track play count: {CurrentTrackPlayCount}");
            }
            var (albumError, albumPlayCount) = await FunctionContainer!.FetchPlayCountsObject.GetAlbumPlayCount(CurrentArtistName, CurrentAlbumName);
            if (!string.IsNullOrEmpty(albumError))
            {
                Logger.Error($"Error fetching album play count: {albumError}");
            }
            else
            {
                CurrentAlbumPlayCount = albumPlayCount;
                Logger.Debug($"Updated album play count: {CurrentAlbumPlayCount}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Error updating play counts.", ex);
        }
    }

    private async Task UpdateLovedInfo()
    {
        if (!CanLoveTracks || string.IsNullOrEmpty(CurrentTrackName) || string.IsNullOrEmpty(CurrentArtistName) || string.IsNullOrEmpty(CurrentAlbumName))
            return;

        try
        {
            Logger.Debug("Updating loved info...");
            var (errorMessage, isLoved) = await FunctionContainer!.LoveTrackObject!.GetLoveState(CurrentArtistName, CurrentTrackName, CurrentAlbumName);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Logger.Error($"Error fetching loved info: {errorMessage}");
                return;
            }

            CurrentTrackLoved = isLoved;
            Logger.Debug($"Updated loved info: {CurrentTrackLoved}");
        }
        catch (Exception ex)
        {
            Logger.Error("Error updating loved info.", ex);
        }
    }

    [RelayCommand]
    private async Task ToggleLovedState()
    {
        if (!CanLoveTracks || string.IsNullOrEmpty(CurrentTrackName) || string.IsNullOrEmpty(CurrentArtistName) || string.IsNullOrEmpty(CurrentAlbumName))
            return;

        try
        {
            Logger.Info($"Setting loved state to {!CurrentTrackLoved}...");
            var errorMessage = await FunctionContainer!.LoveTrackObject!.SetLoveState(CurrentArtistName, CurrentTrackName, CurrentAlbumName, !CurrentTrackLoved);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Logger.Error($"Error setting loved state: {errorMessage}");
                return;
            }

            CurrentTrackLoved = !CurrentTrackLoved;
            Logger.Info($"Set loved state successfully: {CurrentTrackLoved}");
        }
        catch (Exception ex)
        {
            Logger.Error("Error setting loved state.", ex);
        }
    }

    private async Task FetchAlbumArtwork()
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
