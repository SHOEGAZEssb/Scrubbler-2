using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Scrubbler.Plugin.Scrobblers.DatabaseScrobbler;

internal partial class AlbumResultsViewModel(IEnumerable<AlbumResultViewModel> results, bool canGoBack) : ObservableObject, IResultsViewModel
{
    #region Properties

    public ObservableCollection<SearchResultViewModel> Results { get; } = new ObservableCollection<SearchResultViewModel>(results);

    public bool CanGoBack { get; } = canGoBack;

    public event EventHandler? OnGoBackRequested;

    #endregion Properties

    [RelayCommand]
    private void GoBack()
    {
        OnGoBackRequested?.Invoke(this, EventArgs.Empty);
    }
}
