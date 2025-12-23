using MediaPlayerScrobblerBase;
using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Services;
using Scrubbler.Plugins.Scrobblers.MediaPlayerScrobbleBase;
using Shoegaze.LastFM;

namespace Scrubbler.Plugin.Scrobbler.AppleMusicScrobbler;

internal partial class AppleMusicScrobbleViewModel : MediaPlayerScrobblePluginViewModelBase
{
    #region Properties

    /// <summary>
    /// Name of the current track.
    /// </summary>
    public override string CurrentTrackName => _currentSong?.SongName ?? string.Empty;

    /// <summary>
    /// Name of the current artist.
    /// </summary>
    public override string CurrentArtistName => _currentSong?.SongArtist ?? string.Empty;

    /// <summary>
    /// Name of the current album.
    /// </summary>
    public override string CurrentAlbumName => _currentSong?.SongAlbum ?? string.Empty;

    /// <summary>
    /// Duration of the current track in seconds.
    /// </summary>
    public override int CurrentTrackLength => _currentSong?.SongDuration ?? 0;

    private readonly IAppleMusicAutomation _automation;
    private AppleMusicInfo? _currentSong;
    private int _currentSongPlayedSeconds = -1;

    private readonly ITickSource _refreshTicks;
    private readonly ITickSource _countTicks;

    #endregion Properties

    #region Construction

    public AppleMusicScrobbleViewModel(ILastfmClient lastfmClient, ILogService logger, IAppleMusicAutomation automation,
                                       ITickSource refreshTicks, ITickSource countTicks)
        : base(lastfmClient, logger)
    {
        _automation = automation;
        _refreshTicks = refreshTicks;
        _countTicks = countTicks;

        _refreshTicks.Tick += OnRefreshTick;
        _countTicks.Tick += OnCountTick;
    }

    #endregion Construction

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override async Task Connect()
    {
        try
        {
            IsBusy = true;

            CountedSeconds = 0;
            CurrentTrackScrobbled = false;

            _automation.Connect();
            IsConnected = true;

            _currentSong = null;
            UpdateCurrentTrackInfo();

            _refreshTicks.Start();
            _countTicks.Start();
        }
        catch (Exception ex)
        {
            _logger.Error($"Error connecting to Apple Music", ex);
            IsConnected = false;
        }
        finally
        {
            IsBusy = false;
        }
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override async Task Disconnect()
    {
        _refreshTicks.Stop();
        _countTicks.Stop();

        _automation.Disconnect();

        _currentSong = null;
        ClearState();
        IsConnected = false;
    }

    private void OnRefreshTick(object? sender, EventArgs e)
    {
        try
        {
            var newSong = _automation.GetCurrentSong();

            if (newSong == null)
            {
                _currentSong = null;
                return;
            }

            if (_currentSong != newSong)
            {
                _currentSong = newSong;
                CurrentTrackScrobbled = false;
                CountedSeconds = 0;
                _currentSongPlayedSeconds = -1;
                UpdateCurrentTrackInfo();
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Error while getting Apple Music info", ex);
            _ = Disconnect();
        }
    }



    /// <summary>
    /// Counts up and scrobbles if the track has been played longer than 50%.
    /// </summary>
    /// <param name="sender">Ignored.</param>
    /// <param name="e">Ignored.</param>
    private void OnCountTick(object? sender, EventArgs e)
    {
        if (_currentSong == null)
            return;

        var current = _automation.GetCurrentPositionSeconds();
        if (current < 0)
            return;

        if (_currentSongPlayedSeconds != -1 &&
            current != _currentSongPlayedSeconds)
        {
            _ = UpdateNowPlaying();

            if (++CountedSeconds == CurrentTrackLengthToScrobble &&
                CurrentTrackScrobbled == false)
            {
                OnScrobblesDetected([
                    new ScrobbleData(
                    _currentSong.SongName,
                    _currentSong.SongArtist,
                    DateTimeOffset.UtcNow)
                {
                    Album = _currentSong.SongAlbum,
                    AlbumArtist = _currentSong.SongAlbumArtist
                }
                ]);

                CurrentTrackScrobbled = true;
            }
        }

        _currentSongPlayedSeconds = current;
    }
}
