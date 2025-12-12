namespace Scrubbler.Plugin.Scrobblers.DatabaseScrobbler;

internal interface IResultsViewModel
{
    public IEnumerable<SearchResultViewModel> Results { get; }
}
