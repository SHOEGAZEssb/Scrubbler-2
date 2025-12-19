using Moq;
using Scrubbler.Abstractions.Services;
using Scrubbler.Plugin.Scrobblers.DatabaseScrobbler;
using Shoegaze.LastFM;
using Shoegaze.LastFM.Album;
using Shoegaze.LastFM.Artist;

namespace Scrubbler.Test.DatabaseScrobblerTest;

[TestFixture]
public class Tests
{
    private static ArtistInfo[] MakeTestArtistInfos(int num)
    {
        var artistInfos = new ArtistInfo[num];
        for (int i = 0; i < num; i++)
        {
            artistInfos[i] = new ArtistInfo
            {
                Name = $"TestArtist_{i}",
                Url = new Uri("https://example.invalid/resource")
            };
        }

        return artistInfos;
    }

    private static AlbumInfo[] MakeTestAlbumInfos(int num)
    {
        var artistInfos = MakeTestArtistInfos(num);

        var albumInfos = new AlbumInfo[num];
        for (int i = 0; i < num; i++)
        {
            albumInfos[i] = new AlbumInfo
            {
                Name = $"TestAlbum_{i}",
                Artist = artistInfos[i]
            };
        }

        return albumInfos;
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
