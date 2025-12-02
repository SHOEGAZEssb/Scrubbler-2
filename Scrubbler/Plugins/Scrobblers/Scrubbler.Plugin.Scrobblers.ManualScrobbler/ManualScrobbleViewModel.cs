using CommunityToolkit.Mvvm.ComponentModel;
using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Plugin;

namespace Scrubbler.Plugin.Scrobblers.ManualScrobbler;

public partial class ManualScrobbleViewModel : ScrobblePluginViewModelBase
{
    #region Properties

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanScrobble))]
    private string _artistName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanScrobble))]
    private string _trackName = string.Empty;

    [ObservableProperty]
    private string _albumName = string.Empty;

    [ObservableProperty]
    private string _albumArtistName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanScrobble))]
    private DateTimeOffset _playedAt = DateTimeOffset.Now;

    [ObservableProperty]
    private TimeSpan _playedAtTime = DateTimeOffset.Now.TimeOfDay;

    public override bool CanScrobble => !string.IsNullOrEmpty(ArtistName) && !string.IsNullOrEmpty(TrackName);

    #endregion Properties

    public override async Task<IEnumerable<ScrobbleData>> GetScrobblesAsync()
    {
        if (!CanScrobble)
            throw new InvalidOperationException("Invalid data for scrobble creation");

        IsBusy = true;

        try
        {
            return await Task.Run(() =>
            {
                return new[] { new ScrobbleData(TrackName, ArtistName, PlayedAt.Date, PlayedAtTime) };
            });
        }
        finally
        {
            IsBusy = false;
        }
    }
}
