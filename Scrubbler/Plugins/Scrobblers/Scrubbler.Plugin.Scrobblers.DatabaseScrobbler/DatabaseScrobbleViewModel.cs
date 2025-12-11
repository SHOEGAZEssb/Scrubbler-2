using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Services;
using Shoegaze.LastFM;

namespace Scrubbler.Plugin.Scrobblers.DatabaseScrobbler;

public enum Database
{
    Lastfm
}

public enum SearchType
{
    Artist,
    Album
}

public sealed partial class DatabaseScrobbleViewModel(ILogService logger, ILastfmClient lastfmClient) : ScrobbleMultipleViewModelBase<ScrobbableObjectViewModel>
{
    #region Properties

    [ObservableProperty]
    private Database _selectedDatabase = Database.Lastfm;

    [ObservableProperty]
    private SearchType _selectedSearchType = SearchType.Artist;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSearch))]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    private string _searchQuery = string.Empty;

    private bool CanSearch => !string.IsNullOrEmpty(SearchQuery);

    [ObservableProperty]
    private IResultsViewModel? _currentResultVM;
    private ArtistResultsViewModel? _previousArtistResults;

    private readonly ILogService _logger = logger;
    private readonly ILastfmClient _lastfmClient = lastfmClient;

    #endregion Properties

    public override async Task<IEnumerable<ScrobbleData>> GetScrobblesAsync()
    {
        throw new NotImplementedException();
    }

    [RelayCommand]
    private async Task Search()
    {
        if (string.IsNullOrEmpty(SearchQuery))
        {
            _logger.Warn("Search query is empty. Aborting search.");
            return;
        }

        IsBusy = true;

        try
        {
            if (SelectedSearchType == SearchType.Artist)
                await SearchArtistAsync();
            else if (SelectedSearchType == SearchType.Album)
                await SearchAlbumAsync();
        }
        catch (Exception ex)
        {
            _logger.Error("An error occurred while searching the database.", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    #region Search Artist

    private async Task SearchArtistAsync()
    {
        IEnumerable<ArtistResultViewModel> results = [];
        if (SelectedDatabase == Database.Lastfm)
            results = await SearchArtistLastfm();

        if (results.Any())
        {
            // clean up old
            if (CurrentResultVM != null)
            {
                foreach (var result in CurrentResultVM.Results)
                {
                    result.OnClicked -= Result_OnClicked;
                }
            }
            if (_previousArtistResults != null)
            {
                foreach (var result in _previousArtistResults.Results)
                {
                    result.OnClicked -= Result_OnClicked;
                }
            }

            // connect new events
            foreach (var result in results)
            {
                result.OnClicked += Result_OnClicked;
            }

            // cache results so we can go back after we click an artist
            _previousArtistResults = new ArtistResultsViewModel(results);
            CurrentResultVM = _previousArtistResults;
        }
        else
        {
            _logger.Info($"No results found for artist '{SearchQuery}' in database '{SelectedDatabase}'.");
        }
    }

    private async Task<IEnumerable<ArtistResultViewModel>> SearchArtistLastfm()
    {
        var response = await _lastfmClient.Artist.SearchAsync(SearchQuery);
        if (!response.IsSuccess || response.Data == null)
            throw new Exception($"Failed to search artist '{SearchQuery}' on Last.fm: {response.ErrorMessage} | {response.LastFmStatus}");

        return response.Data.Items.Select(a => new ArtistResultViewModel(a.Images.LastOrDefault().Value, a.Name));
    }

    #endregion Search Artist

    #region Search Album

    private async Task SearchAlbumAsync()
    {
        IEnumerable<AlbumResultViewModel> results = [];
        if (SelectedDatabase == Database.Lastfm)
            results = await SearchAlbumLastfm();

        if (results.Any())
        {
            // clean up old
            if (CurrentResultVM != null)
            {
                foreach (var result in CurrentResultVM.Results)
                {
                    result.OnClicked -= Result_OnClicked;
                }
            }

            // connect new events
            foreach (var result in results)
            {
                result.OnClicked += Result_OnClicked;
            }

            CurrentResultVM = new AlbumResultsViewModel(results, canGoBack: false);
        }
        else
        {
            _logger.Info($"No results found for album '{SearchQuery}' in database '{SelectedDatabase}'.");
        }
    }

    private async Task<IEnumerable<AlbumResultViewModel>> SearchAlbumLastfm()
    {
        var response = await _lastfmClient.Album.SearchAsync(SearchQuery);
        if (!response.IsSuccess || response.Data == null)
            throw new Exception($"Failed to search album '{SearchQuery}' on Last.fm: {response.ErrorMessage} | {response.LastFmStatus}");

        return response.Data.Items.Select(a => new AlbumResultViewModel(a.Images.LastOrDefault().Value, a.Name, a.Artist!.Name));
    }

    #endregion Search Album

    private async void Result_OnClicked(object? sender, SearchResultViewModel e)
    {
        if (e is ArtistResultViewModel)
            await ArtistClickedAsync(e.Name);
        else if (e is AlbumResultViewModel al)
            await AlbumClickedAsync(al.Name, al.ArtistName);
    }

    #region Artist Clicked

    private async Task ArtistClickedAsync(string artist)
    {
        IEnumerable<AlbumResultViewModel> results = [];
        if (SelectedDatabase == Database.Lastfm)
            results = await ArtistClickedLastfm(artist);

        if (results.Any())
        {
            // clean up old
            if (CurrentResultVM != null)
            {
                foreach (var result in CurrentResultVM.Results)
                {
                    result.OnClicked -= Result_OnClicked;
                }

                if (CurrentResultVM is AlbumResultsViewModel albumResultsVM)
                    albumResultsVM.OnGoBackRequested -= AlbumResults_GoBack;
            }

            // connect new events
            foreach (var result in results)
            {
                result.OnClicked += Result_OnClicked;
            }

            var vm = new AlbumResultsViewModel(results, canGoBack: true);
            vm.OnGoBackRequested += AlbumResults_GoBack;
            CurrentResultVM = vm;
        }
        else
        {
            _logger.Info($"No albums found for artist '{artist}' in database '{SelectedDatabase}'.");
        }
    }

    private async Task<IEnumerable<AlbumResultViewModel>> ArtistClickedLastfm(string artist)
    {
        var response = await _lastfmClient.Artist.GetTopAlbumsByNameAsync(artist);
        if (!response.IsSuccess || response.Data == null)
            throw new Exception($"Failed to get top albums for artist '{artist}' on Last.fm: {response.ErrorMessage} | {response.LastFmStatus}");

        return response.Data.Items.Select(a => new AlbumResultViewModel(a.Images.LastOrDefault().Value, a.Name, artist));
    }

    private void AlbumResults_GoBack(object? sender, EventArgs e)
    {
        if (_previousArtistResults != null)
            CurrentResultVM = _previousArtistResults;
    }

    #endregion Artist Clicked

    #region Album Clicked

    private async Task AlbumClickedAsync(string album, string artist)
    {
        if (SelectedDatabase == Database.Lastfm)
        {
            var scrobbles = await AlbumClickedLastfm(album, artist);
            Scrobbles = new ObservableCollection<ScrobbableObjectViewModel>(scrobbles);
        }
    }

    private async Task<IEnumerable<ScrobbableObjectViewModel>> AlbumClickedLastfm(string album, string artist)
    {
        var response = await _lastfmClient.Album.GetInfoByNameAsync(album, artist);
        if (!response.IsSuccess || response.Data == null)
            throw new Exception($"Failed to get info for album '{album}' by artist '{artist}' on Last.fm: {response.ErrorMessage} | {response.LastFmStatus}");

        return response.Data.Tracks.Select(t => new ScrobbableObjectViewModel(t.Artist!.Name, t.Name, album, t.Album?.Artist?.Name));
    }

    #endregion Album Clicked
}
