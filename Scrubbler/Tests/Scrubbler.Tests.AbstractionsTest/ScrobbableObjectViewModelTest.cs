using Scrubbler.Abstractions;

namespace Scrubbler.Tests.AbstractionsTest;

[TestFixture]
internal class ScrobbableObjectViewModelTest
{
    [Test]
    public void Constructor_SetsInitialValues()
    {
        var vm = new ScrobbableObjectViewModel(
            artistName: "Artist",
            trackName: "Track",
            albumName: "Album",
            albumArtistName: "AlbumArtist");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(vm.ArtistName, Is.EqualTo("Artist"));
            Assert.That(vm.TrackName, Is.EqualTo("Track"));
            Assert.That(vm.AlbumName, Is.EqualTo("Album"));
            Assert.That(vm.AlbumArtistName, Is.EqualTo("AlbumArtist"));
        }
    }

    [Test]
    public void ToScrobble_Set_RaisesEvent()
    {
        var vm = new ScrobbableObjectViewModel("Artist", "Track");

        var raised = false;
        vm.ToScrobbleChanged += (_, _) => raised = true;

        vm.ToScrobble = true;

        Assert.That(raised, Is.True);
    }

    [Test]
    public void IsSelected_Set_RaisesEvent()
    {
        var vm = new ScrobbableObjectViewModel("Artist", "Track");

        var raised = false;
        vm.IsSelectedChanged += (_, _) => raised = true;

        vm.IsSelected = true;

        Assert.That(raised, Is.True);
    }

    [Test]
    public void UpdateToScrobbleSilent_DoesNotRaiseEvent()
    {
        var vm = new ScrobbableObjectViewModel("Artist", "Track");

        var raised = false;
        vm.ToScrobbleChanged += (_, _) => raised = true;

        vm.UpdateToScrobbleSilent(true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(vm.ToScrobble, Is.True);
            Assert.That(raised, Is.False);
        }
    }

    [Test]
    public void UpdateIsSelectedSilent_DoesNotRaiseEvent()
    {
        var vm = new ScrobbableObjectViewModel("Artist", "Track");

        var raised = false;
        vm.IsSelectedChanged += (_, _) => raised = true;

        vm.UpdateIsSelectedSilent(true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(vm.IsSelected, Is.True);
            Assert.That(raised, Is.False);
        }
    }

    [Test]
    public void ArtistName_DoesNotChange_WhenEmptyOrWhitespace()
    {
        var vm = new ScrobbableObjectViewModel("Artist", "Track")
        {
            ArtistName = ""
        };
        Assert.That(vm.ArtistName, Is.EqualTo("Artist"));

        vm.ArtistName = "   ";
        Assert.That(vm.ArtistName, Is.EqualTo("Artist"));
    }

    [Test]
    public void TrackName_DoesNotChange_WhenEmptyOrWhitespace()
    {
        var vm = new ScrobbableObjectViewModel("Artist", "Track")
        {
            TrackName = ""
        };
        Assert.That(vm.TrackName, Is.EqualTo("Track"));

        vm.TrackName = "   ";
        Assert.That(vm.TrackName, Is.EqualTo("Track"));
    }

    [Test]
    public void AlbumName_AllowsNullAndChange()
    {
        var vm = new ScrobbableObjectViewModel("Artist", "Track")
        {
            AlbumName = "Album"
        };
        Assert.That(vm.AlbumName, Is.EqualTo("Album"));

        vm.AlbumName = null;
        Assert.That(vm.AlbumName, Is.Null);
    }

    [Test]
    public void AlbumArtistName_AllowsNullAndChange()
    {
        var vm = new ScrobbableObjectViewModel("Artist", "Track")
        {
            AlbumArtistName = "AlbumArtist"
        };
        Assert.That(vm.AlbumArtistName, Is.EqualTo("AlbumArtist"));

        vm.AlbumArtistName = null;
        Assert.That(vm.AlbumArtistName, Is.Null);
    }

    [Test]
    public void CanBeScrobbled_DefaultsToTrue()
    {
        var vm = new ScrobbableObjectViewModel("Artist", "Track");

        Assert.That(vm.CanBeScrobbled, Is.True);
    }
}
