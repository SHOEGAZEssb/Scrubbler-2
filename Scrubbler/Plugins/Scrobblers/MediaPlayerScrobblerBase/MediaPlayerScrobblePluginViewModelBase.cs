using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaPlayerScrobblerBase;
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

    public ObservableCollection<TagViewModel> CurrentTrackTags { get; } = [];

    protected readonly ILogService _logger = logger;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanLoveTracks))]
    [NotifyPropertyChangedFor(nameof(CanFetchPlayCounts))]
    [NotifyPropertyChangedFor(nameof(CanFetchTags))]
    [NotifyPropertyChangedFor(nameof(CanOpenLinks))]
    private AccountFunctionContainer? _functionContainer;

    private bool CanLoveTracks => FunctionContainer?.LoveTrackObject != null;

    public bool CanFetchPlayCounts => FunctionContainer?.FetchPlayCountsObject != null;

    public bool CanFetchTags => FunctionContainer?.FetchTagsObject != null;

    public bool CanOpenLinks => FunctionContainer?.OpenLinksObject != null;

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
            _logger.Info("Auto-connect is enabled. Attempting to connect...");
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
        _ = UpdateTags();
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
        // clear old tags
        foreach (var vm in CurrentTrackTags)
        {
            vm.OpenLinkRequested -= Tag_OpenLinkRequested;
        }
        CurrentTrackTags.Clear();
        UpdateCurrentTrackInfo();
    }

    private async Task UpdateNowPlaying()
    {
        if (UpdateNowPlayingObject == null || string.IsNullOrEmpty(CurrentTrackName) || string.IsNullOrEmpty(CurrentArtistName) || string.IsNullOrEmpty(CurrentAlbumName))
            return;

        try
        {
            _logger.Debug("Updating Now Playing...");
            var errorMessage = await UpdateNowPlayingObject.UpdateNowPlaying(CurrentArtistName, CurrentTrackName, CurrentAlbumName);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                _logger.Error($"Error updating Now Playing: {errorMessage}");
                return;
            }
            _logger.Debug("Now Playing updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.Error("Error updating Now Playing.", ex);
        }
    }

    private async Task UpdatePlayCounts()
    {
        if (!CanFetchPlayCounts || string.IsNullOrEmpty(CurrentTrackName) || string.IsNullOrEmpty(CurrentArtistName) || string.IsNullOrEmpty(CurrentAlbumName))
            return;

        try
        {
            _logger.Debug("Updating play counts...");
            var (artistError, artistPlayCount) = await FunctionContainer!.FetchPlayCountsObject!.GetArtistPlayCount(CurrentArtistName);
            if (!string.IsNullOrEmpty(artistError))
            {
                _logger.Error($"Error fetching artist play count: {artistError}");
            }
            else
            {
                CurrentArtistPlayCount = artistPlayCount;
                _logger.Debug($"Updated artist play count: {CurrentArtistPlayCount}");
            }
            var (trackError, trackPlayCount) = await FunctionContainer!.FetchPlayCountsObject.GetTrackPlayCount(CurrentArtistName, CurrentTrackName);
            if (!string.IsNullOrEmpty(trackError))
            {
                _logger.Error($"Error fetching track play count: {trackError}");
            }
            else
            {
                CurrentTrackPlayCount = trackPlayCount;
                _logger.Debug($"Updated track play count: {CurrentTrackPlayCount}");
            }
            var (albumError, albumPlayCount) = await FunctionContainer!.FetchPlayCountsObject.GetAlbumPlayCount(CurrentArtistName, CurrentAlbumName);
            if (!string.IsNullOrEmpty(albumError))
            {
                _logger.Error($"Error fetching album play count: {albumError}");
            }
            else
            {
                CurrentAlbumPlayCount = albumPlayCount;
                _logger.Debug($"Updated album play count: {CurrentAlbumPlayCount}");
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Error updating play counts.", ex);
        }
    }

    private async Task UpdateTags()
    {
        if (!CanFetchTags || string.IsNullOrEmpty(CurrentTrackName) || string.IsNullOrEmpty(CurrentArtistName))
            return;
        try
        {
            _logger.Debug("Updating tags...");
            var (errorMessage, tags) = await FunctionContainer!.FetchTagsObject!.GetTrackTags(CurrentArtistName, CurrentTrackName);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                _logger.Error($"Error fetching tags: {errorMessage}");
                return;
            }

            foreach (var tag in tags)
            {
                var vm = new TagViewModel(tag);
                vm.OpenLinkRequested += Tag_OpenLinkRequested;
                CurrentTrackTags.Add(vm);
            }
            _logger.Debug("Updated tags successfully.");
        }
        catch (Exception ex)
        {
            _logger.Error("Error updating tags.", ex);
        }
    }

    private async void Tag_OpenLinkRequested(object? sender, string e)
    {
        if (!CanOpenLinks)
            return;

        try
        {
            _logger.Debug($"Opening tag link for {e}...");
            await FunctionContainer!.OpenLinksObject!.OpenTagLink(e);
            _logger.Debug("Opened tag link successfully.");
        }
        catch (Exception ex)
        {
            _logger.Error("Error opening tag link.", ex);
        }
    }
    private async Task UpdateLovedInfo()
    {
        if (!CanLoveTracks || string.IsNullOrEmpty(CurrentTrackName) || string.IsNullOrEmpty(CurrentArtistName) || string.IsNullOrEmpty(CurrentAlbumName))
            return;

        try
        {
            _logger.Debug("Updating loved info...");
            var (errorMessage, isLoved) = await FunctionContainer!.LoveTrackObject!.GetLoveState(CurrentArtistName, CurrentTrackName, CurrentAlbumName);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                _logger.Error($"Error fetching loved info: {errorMessage}");
                return;
            }

            CurrentTrackLoved = isLoved;
            _logger.Debug($"Updated loved info: {CurrentTrackLoved}");
        }
        catch (Exception ex)
        {
            _logger.Error("Error updating loved info.", ex);
        }
    }

    [RelayCommand]
    private async Task ToggleLovedState()
    {
        if (!CanLoveTracks || string.IsNullOrEmpty(CurrentTrackName) || string.IsNullOrEmpty(CurrentArtistName) || string.IsNullOrEmpty(CurrentAlbumName))
            return;

        try
        {
            _logger.Info($"Setting loved state to {!CurrentTrackLoved}...");
            var errorMessage = await FunctionContainer!.LoveTrackObject!.SetLoveState(CurrentArtistName, CurrentTrackName, CurrentAlbumName, !CurrentTrackLoved);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                _logger.Error($"Error setting loved state: {errorMessage}");
                return;
            }

            CurrentTrackLoved = !CurrentTrackLoved;
            _logger.Info($"Set loved state successfully: {CurrentTrackLoved}");
        }
        catch (Exception ex)
        {
            _logger.Error("Error setting loved state.", ex);
        }
    }

    [RelayCommand]
    private async Task ArtistClicked()
    {
        if (!CanOpenLinks || string.IsNullOrEmpty(CurrentArtistName))
            return;

        try
        {
            _logger.Debug($"Opening artist link for {CurrentArtistName}...");
            await FunctionContainer!.OpenLinksObject!.OpenArtistLink(CurrentArtistName);
            _logger.Debug("Opened artist link successfully.");
        }
        catch (Exception ex)
        {
            _logger.Error("Error opening artist link.", ex);
        }
    }

    [RelayCommand]
    private async Task AlbumClicked()
    {
        if (!CanOpenLinks || string.IsNullOrEmpty(CurrentArtistName) || string.IsNullOrEmpty(CurrentAlbumName))
            return;

        try
        {
            _logger.Debug($"Opening album link for {CurrentAlbumName} by {CurrentArtistName}...");
            await FunctionContainer!.OpenLinksObject!.OpenAlbumLink(CurrentArtistName, CurrentAlbumName);
            _logger.Debug("Opened album link successfully.");
        }
        catch (Exception ex)
        {
            _logger.Error("Error opening album link.", ex);
        }
    }

    [RelayCommand]
    private async Task TrackClicked()
    {
        if (!CanOpenLinks || string.IsNullOrEmpty(CurrentArtistName) || string.IsNullOrEmpty(CurrentTrackName))
            return;
        try
        {
            _logger.Debug($"Opening track link for {CurrentTrackName} by {CurrentArtistName}...");
            await FunctionContainer!.OpenLinksObject!.OpenTrackLink(CurrentArtistName, CurrentAlbumName, CurrentTrackName);
            _logger.Debug("Opened track link successfully.");
        }
        catch (Exception ex)
        {
            _logger.Error("Error opening track link.", ex);
        }
    }

    private async Task FetchAlbumArtwork()
    {
        if (string.IsNullOrEmpty(CurrentAlbumName) || string.IsNullOrEmpty(CurrentArtistName))
        {
            _logger.Debug("Cannot fetch album artwork: Album name or artist name is empty.");
            CurrentAlbumArtwork = null;
            return;
        }

        var response = await _lastfmClient.Album.GetInfoByNameAsync(CurrentAlbumName, CurrentArtistName);
        if (response.IsSuccess && response.Data != null)
        {
            CurrentAlbumArtwork = response.Data.Images.Values.LastOrDefault();
            _logger.Debug("Fetched album artwork successfully.");
        }
        else
        {
            CurrentAlbumArtwork = null;
            _logger.Debug($"Failed to fetch album artwork: {response.ErrorMessage}");
        }
    }

    protected void OnScrobblesDetected(IEnumerable<ScrobbleData> scrobbles)
    {
        _logger.Info($"Detected {scrobbles.Count()} scrobble(s).");
        ScrobblesDetected?.Invoke(this, scrobbles);
    }
}
