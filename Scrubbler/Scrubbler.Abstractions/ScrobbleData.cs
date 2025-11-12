namespace Scrubbler.Abstractions;

/// <summary>
/// Represents the data for a single scrobble.
/// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrobbleData"/> class.
    /// </summary>
    /// <param name="track">The track name.</param>
    /// <param name="artist">The track artist name.</param>
    /// <param name="timestamp">The date and time the track was played.</param>
    public ScrobbleData(string track, string artist, DateTimeOffset timestamp)
    {
        Track = track;
        Artist = artist;

        // store as UTC DateTimeOffset
        Timestamp = timestamp;
    }
}
