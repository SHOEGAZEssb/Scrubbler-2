using System.Collections.ObjectModel;

namespace Scrubbler.Plugin.Scrobblers.DatabaseScrobbler;

internal interface IResultsViewModel
{
    public ObservableCollection<SearchResultViewModel> Results { get; }
}
