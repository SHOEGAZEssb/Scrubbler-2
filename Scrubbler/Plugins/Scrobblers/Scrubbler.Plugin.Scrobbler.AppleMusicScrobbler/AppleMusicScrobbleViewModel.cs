using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Timers;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.UIA3;
using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Logging;
using Scrubbler.Plugins.Scrobblers.MediaPlayerScrobbleBase;
using Shoegaze.LastFM;
using SystemTimer = System.Timers.Timer;

namespace Scrubbler.Plugin.Scrobbler.AppleMusicScrobbler;

public partial class AppleMusicScrobbleViewModel(ILastfmClient lastfmClient, ILogService logger) : MediaPlayerScrobblePluginViewModelBase(lastfmClient, logger)
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

    private UIA3Automation? _automation;
    private AutomationElement? _songPanel;

    private AppleMusicInfo? _currentSong;
    private int _currentSongPlayedSeconds = -1;

    /// <summary>
    /// Timer to refresh current playing song.
    /// </summary>
    private SystemTimer? _refreshTimer;

    private SystemTimer? _countTimer;

    private static readonly Regex ComposerPerformerRegex = new(@"By\s.*?\s\u2014", RegexOptions.Compiled);
    internal static readonly string[] separator = [" \u2014 "];
    private readonly bool _composerAsArtist = false;

    #endregion Properties

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

            _songPanel = GetSongPanel();
            IsConnected = true;

            _currentSong = null;
            UpdateCurrentTrackInfo();

            _countTimer = new SystemTimer(1000);
            _countTimer.Elapsed += CountTimer_Elapsed;
            _countTimer.Start();

            _refreshTimer = new SystemTimer(1000);
            _refreshTimer.Elapsed += RefreshTimer_Elapsed;
            _refreshTimer.Start();
        }
        catch (Exception ex)
        {
            Logger.Error($"Error connecting to Apple Music: {ex.Message}");
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
        _refreshTimer?.Stop();
        _countTimer?.Stop();
        _automation?.Dispose();
        _automation = null;
        _currentSong = null;
        CountedSeconds = 0;
        CurrentTrackScrobbled = false;
        IsConnected = false;
        //_discordClient.ClearPresence();
        UpdateCurrentTrackInfo();
    }

    private void RefreshTimer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        try
        {
            if (_songPanel == null)
                return;

            var songFieldsPanel = _songPanel.FindFirstChild("LCD");
            var songFields = songFieldsPanel?.FindAllChildren(new ConditionFactory(new UIA3PropertyLibrary())
                                             .ByAutomationId("myScrollViewer")) ?? [];

            if (songFieldsPanel == null || songFields.Length != 2)
            {
                _currentSong = null;
                return;
            }

            var songNameElement = songFields[0];
            var songAlbumArtistElement = songFields[1];

            // the upper rectangle is the song name; the bottom rectangle is the author/album
            // lower .Bottom = higher up on the screen (?)
            if (songNameElement.BoundingRectangle.Bottom > songAlbumArtistElement.BoundingRectangle.Bottom)
            {
                songNameElement = songFields[1];
                songAlbumArtistElement = songFields[0];
            }

            var songName = songNameElement.Name;
            var songAlbumArtist = songAlbumArtistElement.Name;

            string songArtist = "";
            string songAlbum = "";
            string? songPerformer = null;

            // parse song string into album and artist
            try
            {
                var songInfo = ParseSongAlbumArtist(songAlbumArtist, _composerAsArtist);
                songArtist = songInfo.Item1;
                songAlbum = songInfo.Item2;
                songPerformer = songInfo.Item3;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error parsing song album artist: {ex.Message}");
            }

            // get duration from slider
            var s = songFieldsPanel.FindFirstChild("LCDScrubber");
            var duration = s.Patterns.RangeValue.Pattern.Maximum;
            var val = s.Patterns.RangeValue.Pattern.Value;

            var newSong = new AppleMusicInfo(songName, songArtist, songAlbum, songArtist)
            {
                SongDuration = (int)duration.Value,
            };

            // only clear out the current song if song is new
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
            //OnStatusUpdated($"Error while getting Apple Music info: {ex.Message}");
            Disconnect();
        }
    }

    /// <summary>
    /// Counts up and scrobbles if the track has been played longer than 50%.
    /// </summary>
    /// <param name="sender">Ignored.</param>
    /// <param name="e">Ignored.</param>
    private void CountTimer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        if (_songPanel == null || _currentSong == null)
            return;

        var songFieldsPanel = _songPanel.FindFirstChild("LCD");
        if (songFieldsPanel == null)
            return; // no song playing?

        var s = songFieldsPanel.FindFirstChild("LCDScrubber");
        if (s == null)
            return;

        // if current > _currentSongPlayedSeconds, then we are not paused
        var current = (int)s.Patterns.RangeValue.Pattern.Value.Value;
        if (_currentSongPlayedSeconds != -1 && current != _currentSongPlayedSeconds)
        {
            //UpdateNowPlaying().Forget();
            //UpdateRichPresence("applemusic", "Apple Music");

            if (++CountedSeconds == CurrentTrackLengthToScrobble && CurrentTrackScrobbled == false)
            {
                //Scrobble().Forget();
                OnScrobblesDetected([ new ScrobbleData(
                    _currentSong.SongName,
                    _currentSong.SongArtist,
                    DateTimeOffset.UtcNow)
                {
                    Album = _currentSong.SongAlbum,
                    AlbumArtist = _currentSong.SongAlbumArtist
                } ]);
                CurrentTrackScrobbled = true;
            }
        }
        //else
        //    _discordClient.ClearPresence();

        _currentSongPlayedSeconds = current;
    }

    private AutomationElement GetSongPanel()
    {
        var amProcesses = Process.GetProcessesByName("AppleMusic");
        if (amProcesses.Length == 0)
            throw new InvalidOperationException("No Apple Music process found");

        // dispose old automation
        _automation?.Dispose();

        _automation = new UIA3Automation();

        var windows = new List<AutomationElement>();
        var w = _automation.GetDesktop()
          .FindFirstDescendant(c => c.ByProcessId(amProcesses[0].Id));

        if (w != null)
            windows.Add(w);

        // if no windows on the normal desktop, search for virtual desktops and add them
        if (windows.Count == 0)
        {
            var vdesktopWin = FlaUI.Core.Application.Attach(amProcesses[0].Id).GetMainWindow(_automation);
            if (vdesktopWin != null)
                windows.Add(vdesktopWin);
        }

        // get apple music window with info
        AutomationElement? songPanel = null;
        var isMiniPlayer = false;
        foreach (var window in windows)
        {
            isMiniPlayer = window.Name == "Mini Player";
            if (isMiniPlayer)
            {
                songPanel = window.FindFirstChild(cf => cf.ByClassName("Microsoft.UI.Content.DesktopChildSiteBridge"));
                if (songPanel != null)
                    break;
            }
            else
                songPanel = FindFirstDescendantWithAutomationId(window, "TransportBar");
        }

        if (songPanel == null)
            throw new InvalidOperationException("Apple Music song panel is not initialised or missing");

        return songPanel;
    }

    // breadth-first search for element with given automation ID.
    // BFS is preferred as the elements we want to find are generally not too deep in the element tree
    private static AutomationElement? FindFirstDescendantWithAutomationId(AutomationElement baseElement, string id)
    {
        var nodes = new List<AutomationElement>() { baseElement };
        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            if (node.Properties.AutomationId.IsSupported && node.AutomationId == id)
                return node;

            nodes.AddRange(node.FindAllChildren());

            // fallback to prevent this taking too long
            if (nodes.Count > 25)
                return null;
        }

        return null;
    }

    private static Tuple<string, string, string?> ParseSongAlbumArtist(string songAlbumArtist, bool composerAsArtist)
    {
        string songArtist;
        string songAlbum;
        string? songPerformer = null;

        // some classical songs add "By " before the composer's name
        var songComposerPerformer = ComposerPerformerRegex.Matches(songAlbumArtist);
        if (songComposerPerformer.Count > 0)
        {
            var songComposer = songAlbumArtist.Split(separator, StringSplitOptions.None)[0][3..];
            songPerformer = songAlbumArtist.Split(separator, StringSplitOptions.None)[1];
            songArtist = composerAsArtist ? songComposer : songPerformer;
            songAlbum = songAlbumArtist.Split(separator, StringSplitOptions.None)[2];
        }
        else
        {
            // U+2014 is the emdash used by the Apple Music app, not the standard "-" character on the keyboard!
            var songSplit = songAlbumArtist.Split(separator, StringSplitOptions.None);
            if (songSplit.Length > 1)
            {
                songArtist = songSplit[0];
                songAlbum = songSplit[1];
            }
            else
            {
                // no emdash, probably custom music
                // TODO find a better way to handle this?
                songArtist = songSplit[0];
                songAlbum = songSplit[0];
            }
        }

        return new Tuple<string, string, string?>(songArtist, songAlbum, songPerformer);
    }
}
