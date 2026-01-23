using CsvHelper.Configuration;

namespace Scrubbler.Plugin.Scrobbler.FileParseScrobbler.Parser.CSV;

internal sealed class CsvScrobbleRowMap : ClassMap<CsvScrobbleRow>
{
    public CsvScrobbleRowMap(CsvFileParserConfiguration config)
    {
        Map(m => m.Timestamp).Index(config.TimestampFieldIndex);
        Map(m => m.Track).Index(config.TrackFieldIndex);
        Map(m => m.Artist).Index(config.ArtistFieldIndex);
        Map(m => m.Album).Index(config.AlbumFieldIndex);
        Map(m => m.AlbumArtist).Index(config.AlbumArtistFieldIndex);
        Map(m => m.MillisecondsPlayed).Index(config.MillisecondsPlayedFieldIndex);
    }
}
