using Moq;
using Scrubbler.Abstractions;

namespace Scrubbler.Tests.AbstractionsTest.Plugin;

internal static class ScrobbleMockFactory
{
    public static Mock<IScrobbableObjectViewModel> Create(
        bool toScrobble = false,
        bool isSelected = false,
        bool canBeScrobbled = true)
    {
        var mock = new Mock<IScrobbableObjectViewModel>();

        var toScrobbleState = toScrobble;
        var isSelectedState = isSelected;

        // required string properties (not relevant for logic)
        mock.SetupProperty(x => x.TrackName, "Track");
        mock.SetupProperty(x => x.ArtistName, "Artist");
        mock.SetupProperty(x => x.AlbumName, null);
        mock.SetupProperty(x => x.AlbumArtistName, null);

        mock.SetupGet(x => x.CanBeScrobbled).Returns(canBeScrobbled);

        // ToScrobble
        mock.SetupGet(x => x.ToScrobble).Returns(() => toScrobbleState);
        mock.SetupSet(x => x.ToScrobble = It.IsAny<bool>())
            .Callback<bool>(value =>
            {
                if (toScrobbleState != value)
                {
                    toScrobbleState = value;
                    mock.Raise(x => x.ToScrobbleChanged += null, EventArgs.Empty);
                }
            });

        mock.Setup(x => x.UpdateToScrobbleSilent(It.IsAny<bool>()))
            .Callback<bool>(value => toScrobbleState = value);

        // IsSelected
        mock.SetupGet(x => x.IsSelected).Returns(() => isSelectedState);
        mock.SetupSet(x => x.IsSelected = It.IsAny<bool>())
            .Callback<bool>(value =>
            {
                if (isSelectedState != value)
                {
                    isSelectedState = value;
                    mock.Raise(x => x.IsSelectedChanged += null, EventArgs.Empty);
                }
            });

        mock.Setup(x => x.UpdateIsSelectedSilent(It.IsAny<bool>()))
            .Callback<bool>(value => isSelectedState = value);

        return mock;
    }
}
