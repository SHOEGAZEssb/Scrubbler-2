using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Scrubbler.Plugin.Scrobblers.DatabaseScrobbler;

internal partial class AlbumResultsViewModel(IEnumerable<AlbumResultViewModel> results, bool canGoBack) : ObservableObject, IResultsViewModel
{
    #region Properties

    public IEnumerable<SearchResultViewModel> Results => TypedResults;

    public ObservableCollection<AlbumResultViewModel> TypedResults { get; } = new ObservableCollection<AlbumResultViewModel>(results);

    public bool CanGoBack { get; } = canGoBack;

    public event EventHandler? OnGoBackRequested;

    #endregion Properties

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void GoBack()
    {
        OnGoBackRequested?.Invoke(this, EventArgs.Empty);
    }
}
