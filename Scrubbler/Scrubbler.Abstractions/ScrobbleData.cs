namespace Scrubbler.Abstractions;

/// <summary>
/// Represents the data for a single scrobble.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ScrobbleData"/> class.
/// </remarks>
public class ScrobbleData
{
    /// <summary>
    /// Gets or sets the track name.
    /// </summary>
    public string Track { get; set; }

    /// <summary>
    /// Gets or sets the track artist name.
    /// </summary>
    public string Artist { get; set; }

    /// <summary>
    /// Gets or sets the album name, if available.
    /// </summary>
    public string? Album { get; set; }

    /// <summary>
    /// Gets or sets the album artist name, if available.
    /// </summary>
    public string? AlbumArtist { get; set; }

    /// <summary>
    /// Gets the timestamp (UTC) the track was played.
    /// </summary>
    public DateTimeOffset Timestamp { get; }
    /// <param name="track">The track name.</param>
    /// <param name="artist">The track artist name.</param>
    /// <param name="timestamp">The date and time the track was played.</param>
    public ScrobbleData(string track, string artist, DateTimeOffset timestamp)
    {
        if (string.IsNullOrEmpty(track))
            throw new ArgumentException("Track name cannot be null or empty.", nameof(track));
        if (string.IsNullOrEmpty(artist))
            throw new ArgumentException("Artist name cannot be null or empty.", nameof(artist));

        Track = track;
        Artist = artist;
        Timestamp = timestamp;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrobbleData"/> class.
    /// </summary>
    /// <param name="track">The track name.</param>
    /// <param name="artist">The track artist name.</param>
    /// <param name="playedAt">The date the track was played.</param>
    /// <param name="playedAtTime">The time of day the track was played.</param>
    public ScrobbleData(string track, string artist, DateTime playedAt, TimeSpan playedAtTime)
        : this(track, artist, playedAt.Date + playedAtTime)
    { }

    public static IEnumerable<ScrobbleData> FromMasterTimestamp(IEnumerable<IScrobbableObjectViewModel> scrobbles, DateTimeOffset masterTimestamp, bool reverse, int secondsToSubtract = 180)
    {
        var results = new List<ScrobbleData>();
        if (reverse)
            scrobbles = scrobbles.Reverse();

        foreach (var scrobble in scrobbles.ToList())
        {
            results.Add(new ScrobbleData(scrobble.TrackName, scrobble.ArtistName, masterTimestamp) { Album = scrobble.AlbumName, AlbumArtist = scrobble.AlbumArtistName });
            masterTimestamp = masterTimestamp.Subtract(TimeSpan.FromSeconds(secondsToSubtract));
        }

        return results;
    }
}
