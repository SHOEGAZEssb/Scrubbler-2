using Moq;
using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Services;
using Scrubbler.Plugin.Scrobbler.AppleMusicScrobbler;
using Shoegaze.LastFM;

namespace Scrubbler.Test.AppleMusicScrobblerTest;

[TestFixture]
public class Tests
{
    private Mock<ILastfmClient> _lastfmClient = null!;
    private Mock<ILogService> _logger = null!;
    private Mock<IAppleMusicAutomation> _automation = null!;

    private ManualTickSource _refreshTicks = null!;
    private ManualTickSource _countTicks = null!;

    private AppleMusicScrobbleViewModel _vm = null!;

    private readonly AppleMusicInfo _song =
        new("Track", "Artist", "Album", "Artist")
        {
            SongDuration = 100
        };

    [SetUp]
    public void Setup()
    {
        _lastfmClient = new Mock<ILastfmClient>();
        _logger = new Mock<ILogService>();
        _automation = new Mock<IAppleMusicAutomation>(MockBehavior.Strict);

        _refreshTicks = new ManualTickSource();
        _countTicks = new ManualTickSource();

        _vm = new AppleMusicScrobbleViewModel(
            _lastfmClient.Object,
            _logger.Object,
            _automation.Object,
            _refreshTicks,
            _countTicks);
    }

    [TearDown]
    public void TearDown()
    {
        _refreshTicks.Dispose();
        _countTicks.Dispose();
    }

    [Test]
    public async Task Connect_initializes_state_and_connects_automation()
    {
        _automation.Setup(a => a.Connect());

        await _vm.ToggleConnectionCommand.ExecuteAsync(null);

        _automation.Verify(a => a.Connect(), Times.Once);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_vm.IsConnected, Is.True);
            Assert.That(_vm.CurrentTrackScrobbled, Is.False);
            Assert.That(_vm.CountedSeconds, Is.Zero);
        }
    }

    [Test]
    public async Task RefreshTick_sets_current_song()
    {
        _automation.Setup(a => a.Connect());
        _automation.Setup(a => a.GetCurrentSong()).Returns(_song);

        await _vm.ToggleConnectionCommand.ExecuteAsync(null);

        _refreshTicks.Fire();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_vm.CurrentTrackName, Is.EqualTo(_song.SongName));
            Assert.That(_vm.CurrentArtistName, Is.EqualTo(_song.SongArtist));
            Assert.That(_vm.CurrentAlbumName, Is.EqualTo(_song.SongAlbum));
        }
    }

    [Test]
    public async Task RefreshTick_song_change_resets_counters()
    {
        var song2 = new AppleMusicInfo("Other", "Artist", "Album", "Artist")
        {
            SongDuration = 200
        };

        _automation.Setup(a => a.Connect());
        _automation.SetupSequence(a => a.GetCurrentSong())
            .Returns(_song)
            .Returns(song2);

        await _vm.ToggleConnectionCommand.ExecuteAsync(null);

        _refreshTicks.Fire(); // first song
        _vm.CountedSeconds = 10;
        _vm.CurrentTrackScrobbled = true;

        _refreshTicks.Fire(); // song changed

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_vm.CountedSeconds, Is.Zero);
            Assert.That(_vm.CurrentTrackScrobbled, Is.False);
            Assert.That(_vm.CurrentTrackName, Is.EqualTo("Other"));
        }
    }

    [Test]
    public async Task CountTick_increments_when_playback_advances()
    {
        _automation.Setup(a => a.Connect());
        _automation.Setup(a => a.GetCurrentSong()).Returns(_song);

        _automation.SetupSequence(a => a.GetCurrentPositionSeconds())
            .Returns(1)
            .Returns(2)
            .Returns(3);

        await _vm.ToggleConnectionCommand.ExecuteAsync(null);
        _refreshTicks.Fire();

        _countTicks.Fire();
        _countTicks.Fire();
        _countTicks.Fire();

        Assert.That(_vm.CountedSeconds, Is.EqualTo(2));
    }

    [Test]
    public async Task CountTick_does_not_increment_when_paused()
    {
        _automation.Setup(a => a.Connect());
        _automation.Setup(a => a.GetCurrentSong()).Returns(_song);

        _automation.Setup(a => a.GetCurrentPositionSeconds()).Returns(5);

        await _vm.ToggleConnectionCommand.ExecuteAsync(null);
        _refreshTicks.Fire();

        _countTicks.Fire();
        _countTicks.Fire();
        _countTicks.Fire();

        Assert.That(_vm.CountedSeconds, Is.Zero);
    }

    [Test]
    public async Task Scrobble_is_emitted_after_50_percent_played()
    {
        var scrobbles = new List<ScrobbleData>();

        _vm.ScrobblesDetected += (_, data) =>
            scrobbles.AddRange(data);

        _automation.Setup(a => a.Connect());
        _automation.Setup(a => a.GetCurrentSong()).Returns(_song);

        // simulate playback moving forward every tick
        _automation.SetupSequence(a => a.GetCurrentPositionSeconds())
            .Returns(1)
            .Returns(2)
            .Returns(3)
            .Returns(4)
            .Returns(5)
            .Returns(6)
            .Returns(7)
            .Returns(8)
            .Returns(9)
            .Returns(10)
            .Returns(11)
            .Returns(12)
            .Returns(13)
            .Returns(14)
            .Returns(15)
            .Returns(16)
            .Returns(17)
            .Returns(18)
            .Returns(19)
            .Returns(20)
            .Returns(21)
            .Returns(22)
            .Returns(23)
            .Returns(24)
            .Returns(25)
            .Returns(26)
            .Returns(27)
            .Returns(28)
            .Returns(29)
            .Returns(30)
            .Returns(31)
            .Returns(32)
            .Returns(33)
            .Returns(34)
            .Returns(35)
            .Returns(36)
            .Returns(37)
            .Returns(38)
            .Returns(39)
            .Returns(40)
            .Returns(41)
            .Returns(42)
            .Returns(43)
            .Returns(44)
            .Returns(45)
            .Returns(46)
            .Returns(47)
            .Returns(48)
            .Returns(49)
            .Returns(50)
            .Returns(51);

        await _vm.ToggleConnectionCommand.ExecuteAsync(null);
        _refreshTicks.Fire();

        for (var i = 0; i < _vm.CurrentTrackLengthToScrobble + 1; i++)
            _countTicks.Fire();

        Assert.That(scrobbles, Has.Count.EqualTo(1));
        Assert.That(scrobbles[0].Track, Is.EqualTo("Track"));
    }
}
