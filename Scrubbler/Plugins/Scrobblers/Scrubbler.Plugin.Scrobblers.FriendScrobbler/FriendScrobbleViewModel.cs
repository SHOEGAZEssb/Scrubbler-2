using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Plugin;
using Shoegaze.LastFM;

namespace Scrubbler.Plugin.Scrobblers.FriendScrobbler;

internal partial class FriendScrobbleViewModel(ILastfmClient lastfmClient) : ScrobbleMultipleViewModelBase<FetchedScrobbleViewModel>
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(FetchCommand))]
    private string _userName = string.Empty;

    public bool CanFetch => !string.IsNullOrEmpty(UserName);

    public override bool CanScrobble => Scrobbles.Any(s => s.ToScrobble);

    private readonly ILastfmClient _lastfmClient = lastfmClient;

    public override async Task<IEnumerable<ScrobbleData>> GetScrobblesAsync()
    {
        return await Task.Run(() => Scrobbles.Where(s => s.ToScrobble)
                                       .Select(s => new ScrobbleData(s.TrackName, s.ArtistName, s.Timestamp) { Album = s.AlbumName, AlbumArtist = s.AlbumArtistName }));
    }

    [RelayCommand(CanExecute = nameof(CanFetch))]
    private async Task Fetch()
    {
        IsBusy = true;

        try
        {
            var response = await _lastfmClient.User.GetRecentTracksAsync(UserName, extended: true, ignoreNowPlaying: true, limit: Limit);
            if (!response.IsSuccess || response.Data == null)
            {
                // todo throw
                return;
            }

            Scrobbles.Clear();
            Scrobbles = new ObservableCollection<FetchedScrobbleViewModel>(response.Data.Items.Select(
                s => new FetchedScrobbleViewModel(new ScrobbleData(s.Name, s.Artist!.Name, s.PlayedAtUtc!.Value) { Album = s.Album?.Name, AlbumArtist = s.Album?.Artist?.Name })));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [ObservableProperty]
    private int _limit = 50;
}
