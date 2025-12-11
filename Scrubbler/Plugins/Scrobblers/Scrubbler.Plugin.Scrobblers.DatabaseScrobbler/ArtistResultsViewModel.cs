using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Scrubbler.Plugin.Scrobblers.DatabaseScrobbler;

internal class ArtistResultsViewModel(IEnumerable<ArtistResultViewModel> results) : ObservableObject, IResultsViewModel
{
    #region Properties

    public ObservableCollection<SearchResultViewModel> Results { get; } = new ObservableCollection<SearchResultViewModel>(results);

    #endregion Properties
}
