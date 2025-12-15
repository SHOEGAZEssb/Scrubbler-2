using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Scrubbler.Plugin.Scrobblers.DatabaseScrobbler;

internal class ArtistResultsViewModel(IEnumerable<ArtistResultViewModel> results) : ObservableObject, IResultsViewModel
{
    #region Properties

    public IEnumerable<SearchResultViewModel> Results => TypedResults;

    public ObservableCollection<ArtistResultViewModel> TypedResults { get; } = new ObservableCollection<ArtistResultViewModel>(results);

    #endregion Properties
}
