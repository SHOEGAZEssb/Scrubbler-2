namespace Scrubbler.Plugin.Scrobblers.DatabaseScrobbler;

internal sealed class ResultsTemplateSelector : DataTemplateSelector
{
    public DataTemplate? ArtistTemplate { get; set; }
    public DataTemplate? AlbumTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
        => item switch
        {
            ArtistResultsViewModel => ArtistTemplate,
            AlbumResultsViewModel => AlbumTemplate,
            _ => null
        };
}
