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
    private int _amount = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanScrobble))]
    private DateTimeOffset _playedAt = DateTimeOffset.Now;

    [ObservableProperty]
    private TimeSpan _playedAtTime = DateTimeOffset.Now.TimeOfDay;

    public override bool CanScrobble => !string.IsNullOrEmpty(ArtistName) && !string.IsNullOrEmpty(TrackName) && Amount > 0 && Amount <= 3000;

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
                var scrobbles = new ScrobbleData[Amount];
                for (int i = 0; i < scrobbles.Length; i++)
                {
                    scrobbles[i] = new ScrobbleData(TrackName, ArtistName, PlayedAt.Date, PlayedAtTime);
                }

                return scrobbles;
            });
        }
        finally
        {
            IsBusy = false;
        }
    }
}
