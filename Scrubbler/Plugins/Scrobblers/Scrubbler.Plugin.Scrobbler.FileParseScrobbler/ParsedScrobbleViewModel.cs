using Scrubbler.Abstractions;

namespace Scrubbler.Plugin.Scrobbler.FileParseScrobbler;

internal partial class ParsedScrobbleViewModel(ScrobbleData scrobble) : ScrobbableObjectViewModel(scrobble.Artist, scrobble.Track, scrobble.Album, scrobble.AlbumArtist)
{
    #region Properties

    public override bool CanBeScrobbled => Timestamp > DateTime.Now.Subtract(TimeSpan.FromDays(14));

    public DateTimeOffset Timestamp => _scrobble.Timestamp;

    private readonly ScrobbleData _scrobble = scrobble;

    #endregion Properties
}
