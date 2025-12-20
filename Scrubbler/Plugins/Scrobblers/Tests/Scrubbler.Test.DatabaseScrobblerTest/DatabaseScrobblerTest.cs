using Moq;
using Scrubbler.Abstractions.Services;
using Scrubbler.Plugin.Scrobblers.DatabaseScrobbler;
using Shoegaze.LastFM;
using Shoegaze.LastFM.Album;
using Shoegaze.LastFM.Artist;
using Shoegaze.LastFM.Track;

namespace Scrubbler.Test.DatabaseScrobblerTest;

[TestFixture]
public class Tests
{
    private static ArtistInfo[] MakeTestArtistInfos(int num, string artistName = "TestArtist")
    {
        var artistInfos = new ArtistInfo[num];
        for (int i = 0; i < num; i++)
        {
            artistInfos[i] = new ArtistInfo
            {
                Name = $"{artistName}_{i}",
                Url = new Uri("https://example.invalid/resource")
            };
        }

        return artistInfos;
    }

    private static AlbumInfo[] MakeTestAlbumInfos(int num, string albumName = "TestAlbum", string artistName = "TestArtist")
    {
        var artistInfos = MakeTestArtistInfos(num, artistName);

        var albumInfos = new AlbumInfo[num];
        for (int i = 0; i < num; i++)
        {
            albumInfos[i] = new AlbumInfo
            {
                Name = $"{albumName}_{i}",
                Artist = artistInfos[i]
            };
            albumInfos[i].Tracks = MakeTestTrackInfos(5, artistInfos[i], albumInfos[i]);
        }

        return albumInfos;
    }

    private static TrackInfo[] MakeTestTrackInfos(int num, ArtistInfo artistInfo, AlbumInfo albumInfo, string trackName = "TestTrack")
    {
        var trackInfos = new TrackInfo[num];
        for (int i = 0; i < num; i++)
        {
            trackInfos[i] = new TrackInfo
            {
                Name = $"{trackName}_{i}",
                Url = new Uri("https://example.invalid/resource"),
                Artist = artistInfo,
                Album = albumInfo
            };
        }

        return trackInfos;
    }

    [Test]
    public async Task SearchArtistLastFmTest()
    {
        // mock setup
        var logMock = new Mock<ILogService>();

        var ai = MakeTestArtistInfos(3);
        var pagedResult = new PagedResult<ArtistInfo>(ai, 1, 1, ai.Length, ai.Length);
        var response = new ApiResult<PagedResult<ArtistInfo>>(pagedResult, LastFmStatusCode.Success, System.Net.HttpStatusCode.OK, null);

        var artistMock = new Mock<IArtistApi>(MockBehavior.Strict);
        artistMock.Setup(a => a.SearchAsync("TestArtist", It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);

        var clientMock = new Mock<ILastfmClient>(MockBehavior.Strict);
        clientMock.Setup(c => c.Artist).Returns(artistMock.Object);

        // actual test
        var vm = new DatabaseScrobbleViewModel(logMock.Object, clientMock.Object)
        {
            SearchQuery = "TestArtist",
            SelectedDatabase = Database.Lastfm,
            SelectedSearchType = SearchType.Artist
        };

        await vm.SearchCommand.ExecuteAsync(null);

        var resultVM = vm.CurrentResultVM as ArtistResultsViewModel;
        Assert.That(resultVM, Is.Not.Null);
        Assert.That(resultVM.TypedResults, Has.Count.EqualTo(ai.Length));

        for (int i = 0; i < ai.Length; i++)
        {
            Assert.That(resultVM.TypedResults[i].Name, Is.EqualTo(ai[i].Name));
        }
    }

    [Test]
    public async Task SearchAlbumLastFmTest()
    {
        // mock setup
        var logMock = new Mock<ILogService>();

        var ai = MakeTestAlbumInfos(3);
        var pagedResult = new PagedResult<AlbumInfo>(ai, 1, 1, ai.Length, ai.Length);
        var response = new ApiResult<PagedResult<AlbumInfo>>(pagedResult, LastFmStatusCode.Success, System.Net.HttpStatusCode.OK, null);

        var albumMock = new Mock<IAlbumApi>(MockBehavior.Strict);
        albumMock.Setup(a => a.SearchAsync("TestArtist", It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);

        var clientMock = new Mock<ILastfmClient>(MockBehavior.Strict);
        clientMock.Setup(c => c.Album).Returns(albumMock.Object);

        // actual test
        var vm = new DatabaseScrobbleViewModel(logMock.Object, clientMock.Object)
        {
            SearchQuery = "TestArtist",
            SelectedDatabase = Database.Lastfm,
            SelectedSearchType = SearchType.Album
        };

        await vm.SearchCommand.ExecuteAsync(null);

        var resultVM = vm.CurrentResultVM as AlbumResultsViewModel;
        Assert.That(resultVM, Is.Not.Null);
        Assert.That(resultVM.TypedResults, Has.Count.EqualTo(ai.Length));

        for (int i = 0; i < ai.Length; i++)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(resultVM.TypedResults[i].Name, Is.EqualTo(ai[i].Name));
                Assert.That(resultVM.TypedResults[i].ArtistName, Is.EqualTo(ai[i].Artist!.Name));
                Assert.That(resultVM.CanGoBack, Is.False);
            }
        }
    }

    [Test]
    public async Task ClickArtistLastFmTest()
    {
        // mock setup
        var logMock = new Mock<ILogService>();

        var artistInfos = MakeTestArtistInfos(3);
        var pagedArtistResult = new PagedResult<ArtistInfo>(artistInfos, 1, 1, artistInfos.Length, artistInfos.Length);
        var searchResponse = new ApiResult<PagedResult<ArtistInfo>>(pagedArtistResult, LastFmStatusCode.Success, System.Net.HttpStatusCode.OK, null);

        var albumInfos = MakeTestAlbumInfos(2);
        var pagedAlbumResult = new PagedResult<AlbumInfo>(albumInfos, 1, 1, albumInfos.Length, albumInfos.Length);
        var topAlbumsResponse = new ApiResult<PagedResult<AlbumInfo>>(pagedAlbumResult, LastFmStatusCode.Success, System.Net.HttpStatusCode.OK, null);

        var artistMock = new Mock<IArtistApi>(MockBehavior.Strict);
        artistMock.Setup(a => a.SearchAsync("TestArtist", It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResponse);
        artistMock.Setup(a => a.GetTopAlbumsByNameAsync("TestArtist_0", It.IsAny<bool>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>())).ReturnsAsync(topAlbumsResponse);

        var clientMock = new Mock<ILastfmClient>(MockBehavior.Strict);
        clientMock.Setup(c => c.Artist).Returns(artistMock.Object);

        // actual test
        var vm = new DatabaseScrobbleViewModel(logMock.Object, clientMock.Object)
        {
            SearchQuery = "TestArtist",
            SelectedDatabase = Database.Lastfm,
            SelectedSearchType = SearchType.Artist
        };

        await vm.SearchCommand.ExecuteAsync(null);

        var results = vm.CurrentResultVM as ArtistResultsViewModel;
        Assert.That(results, Is.Not.Null);

        results.TypedResults[0].ClickedCommand.Execute(null);

        var albumResults = vm.CurrentResultVM as AlbumResultsViewModel;
        Assert.That(albumResults, Is.Not.Null);

        Assert.That(albumResults.TypedResults, Has.Count.EqualTo(albumInfos.Length));

        for (int i = 0; i < albumInfos.Length; i++)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(albumResults.TypedResults[i].Name, Is.EqualTo(albumInfos[i].Name));
                Assert.That(albumResults.TypedResults[i].ArtistName, Is.EqualTo(artistInfos[0].Name));
                Assert.That(albumResults.CanGoBack, Is.True);
            }
        }
    }

    [Test]
    public async Task ClickAlbumLastFmTest()
    {
        // mock setup
        var logMock = new Mock<ILogService>();

        var ai = MakeTestAlbumInfos(3);
        var pagedResult = new PagedResult<AlbumInfo>(ai, 1, 1, ai.Length, ai.Length);
        var searchResponse = new ApiResult<PagedResult<AlbumInfo>>(pagedResult, LastFmStatusCode.Success, System.Net.HttpStatusCode.OK, null);
        var infoResponse = new ApiResult<AlbumInfo>(ai[1], LastFmStatusCode.Success, System.Net.HttpStatusCode.OK, null);

        var albumMock = new Mock<IAlbumApi>(MockBehavior.Strict);
        albumMock.Setup(a => a.SearchAsync("TestArtist", It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>())).ReturnsAsync(searchResponse);
        albumMock.Setup(a => a.GetInfoByNameAsync("TestAlbum_1", "TestArtist_1", It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync(infoResponse);

        var clientMock = new Mock<ILastfmClient>(MockBehavior.Strict);
        clientMock.Setup(c => c.Album).Returns(albumMock.Object);

        // actual test
        var vm = new DatabaseScrobbleViewModel(logMock.Object, clientMock.Object)
        {
            SearchQuery = "TestArtist",
            SelectedDatabase = Database.Lastfm,
            SelectedSearchType = SearchType.Album
        };

        await vm.SearchCommand.ExecuteAsync(null);

        var resultVM = vm.CurrentResultVM as AlbumResultsViewModel;
        Assert.That(resultVM, Is.Not.Null);

        resultVM.TypedResults[1].ClickedCommand.Execute(null);

        var trackResultVM = vm.CurrentResultVM as DatabaseScrobbleViewModel;
        Assert.That(trackResultVM, Is.Not.Null);
        Assert.That(trackResultVM.Scrobbles, Has.Count.EqualTo(ai[1].Tracks.Count));

        for (int i = 0; i < ai[1].Tracks.Count; i++)
        {
            var testTrack = ai[1].Tracks[i];
            var s = trackResultVM.Scrobbles[i];

            using (Assert.EnterMultipleScope())
            {
                Assert.That(s.TrackName, Is.EqualTo(testTrack.Name));
                Assert.That(s.ArtistName, Is.EqualTo(testTrack.Artist!.Name));
                Assert.That(s.AlbumName, Is.EqualTo(testTrack.Album!.Name));
                Assert.That(s.ToScrobble, Is.False);
            }

            s.ToScrobble = true;
        }

        var scrobbles = await trackResultVM.GetScrobblesAsync();
        Assert.That(scrobbles.Count(), Is.EqualTo(ai[1].Tracks.Count));
    }

    [Test]
    public void CanSearchTest()
    {
        // mock setup
        var logMock = new Mock<ILogService>();
        var clientMock = new Mock<ILastfmClient>(MockBehavior.Strict);

        var vm = new DatabaseScrobbleViewModel(logMock.Object, clientMock.Object);

        Assert.That(vm.SearchCommand.CanExecute(null), Is.False);
        vm.SearchQuery = "Test";
        Assert.That(vm.SearchCommand.CanExecute(null), Is.True);
        vm.SearchQuery = string.Empty;
        Assert.That(vm.SearchCommand.CanExecute(null), Is.False);
    }
}
