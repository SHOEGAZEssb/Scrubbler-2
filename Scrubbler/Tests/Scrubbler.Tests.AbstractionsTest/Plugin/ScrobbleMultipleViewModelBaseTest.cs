using Moq;
using Scrubbler.Abstractions;

namespace Scrubbler.Tests.AbstractionsTest.Plugin;

[TestFixture]
internal class ScrobbleMultipleViewModelBaseTest
{
    private static Mock<IScrobbableObjectViewModel> CreateScrobbleMock(
        bool toScrobble = false,
        bool isSelected = false,
        bool canBeScrobbled = true)
    {
        var mock = new Mock<IScrobbableObjectViewModel>();

        var toScrobbleField = toScrobble;
        var isSelectedField = isSelected;

        // required string properties
        mock.SetupProperty(x => x.TrackName, "Track");
        mock.SetupProperty(x => x.ArtistName, "Artist");
        mock.SetupProperty(x => x.AlbumName, null);
        mock.SetupProperty(x => x.AlbumArtistName, null);

        mock.SetupGet(x => x.CanBeScrobbled).Returns(canBeScrobbled);

        // ToScrobble
        mock.SetupGet(x => x.ToScrobble).Returns(() => toScrobbleField);
        mock.SetupSet(x => x.ToScrobble = It.IsAny<bool>())
            .Callback<bool>(value =>
            {
                if (toScrobbleField != value)
                {
                    toScrobbleField = value;
                    mock.Raise(x => x.ToScrobbleChanged += null, EventArgs.Empty);
                }
            });

        mock.Setup(x => x.UpdateToScrobbleSilent(It.IsAny<bool>()))
            .Callback<bool>(value => toScrobbleField = value);

        // IsSelected
        mock.SetupGet(x => x.IsSelected).Returns(() => isSelectedField);
        mock.SetupSet(x => x.IsSelected = It.IsAny<bool>())
            .Callback<bool>(value =>
            {
                if (isSelectedField != value)
                {
                    isSelectedField = value;
                    mock.Raise(x => x.IsSelectedChanged += null, EventArgs.Empty);
                }
            });

        mock.Setup(x => x.UpdateIsSelectedSilent(It.IsAny<bool>()))
            .Callback<bool>(value => isSelectedField = value);

        return mock;
    }

    [Test]
    public void CanScrobble_True_WhenAnyItemIsMarked()
    {
        var vm = new TestScrobbleMultipleViewModel();
        vm.SetScrobbles(
        [
            CreateScrobbleMock().Object,
            CreateScrobbleMock(toScrobble: true).Object
        ]);

        Assert.That(vm.CanScrobble, Is.True);
    }

    [Test]
    public void CheckAll_MarksAllAsToScrobble()
    {
        var s1 = CreateScrobbleMock();
        var s2 = CreateScrobbleMock();

        var vm = new TestScrobbleMultipleViewModel();
        vm.SetScrobbles([s1.Object, s2.Object]);

        vm.CheckAllCommand.Execute(null);

        Assert.That(vm.Scrobbles.All(s => s.ToScrobble), Is.True);
        Assert.That(vm.ToScrobbleCount, Is.EqualTo(2));
    }

    [Test]
    public void UncheckAll_ClearsAllToScrobble()
    {
        var vm = new TestScrobbleMultipleViewModel();
        vm.SetScrobbles(
        [
            CreateScrobbleMock(toScrobble: true).Object,
            CreateScrobbleMock(toScrobble: true).Object
        ]);

        vm.UncheckAllCommand.Execute(null);

        Assert.That(vm.Scrobbles.All(s => !s.ToScrobble), Is.True);
        Assert.That(vm.ToScrobbleCount, Is.EqualTo(0));
    }

    [Test]
    public void CheckSelected_OnlyAffectsSelectedItems()
    {
        var selected = CreateScrobbleMock(isSelected: true);
        var notSelected = CreateScrobbleMock(isSelected: false);

        var vm = new TestScrobbleMultipleViewModel();
        vm.SetScrobbles([selected.Object, notSelected.Object]);

        vm.CheckSelectedCommand.Execute(null);

        Assert.That(selected.Object.ToScrobble, Is.True);
        Assert.That(notSelected.Object.ToScrobble, Is.False);
    }

    [Test]
    public void UncheckSelected_OnlyClearsSelectedItems()
    {
        var selected = CreateScrobbleMock(toScrobble: true, isSelected: true);
        var notSelected = CreateScrobbleMock(toScrobble: true, isSelected: false);

        var vm = new TestScrobbleMultipleViewModel();
        vm.SetScrobbles([selected.Object, notSelected.Object]);

        vm.UncheckSelectedCommand.Execute(null);

        Assert.That(selected.Object.ToScrobble, Is.False);
        Assert.That(notSelected.Object.ToScrobble, Is.True);
    }

    [Test]
    public void SelectedCount_Updates_WhenIsSelectedChanges()
    {
        var scrobble = CreateScrobbleMock();

        var vm = new TestScrobbleMultipleViewModel();
        vm.SetScrobbles([scrobble.Object]);

        scrobble.Object.IsSelected = true;

        Assert.That(vm.SelectedCount, Is.EqualTo(1));
    }

    [Test]
    public void ReplacingScrobbles_RewiresEventsCorrectly()
    {
        var oldScrobble = CreateScrobbleMock();
        var newScrobble = CreateScrobbleMock();

        var vm = new TestScrobbleMultipleViewModel();
        vm.SetScrobbles([oldScrobble.Object]);

        vm.SetScrobbles([newScrobble.Object]);

        newScrobble.Object.ToScrobble = true;

        Assert.That(vm.CanScrobble, Is.True);
    }
}
