namespace Scrubbler.Plugin.Scrobblers.DatabaseScrobbler;

internal sealed class ResultsTemplateSelector : DataTemplateSelector
{
    public DataTemplate? ArtistTemplate { get; set; }
    public DataTemplate? AlbumTemplate { get; set; }

    public DataTemplate? TrackTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
        => item switch
        {
            ArtistResultsViewModel => ArtistTemplate,
            AlbumResultsViewModel => AlbumTemplate,
            DatabaseScrobbleViewModel => TrackTemplate,
            _ => null
        };
}
