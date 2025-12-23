using System.Text;

namespace Scrubbler.Plugin.Scrobbler.FileParseScrobbler.Parser.CSV;

/// <summary>
/// Immutable configuration snapshot for CSV scrobble parsing.
/// </summary>
internal sealed record CsvFileParserConfiguration : IFileParserConfiguration
{
    /// <summary>
    /// Encoding code page used to read the CSV file.
    /// </summary>
    public required int EncodingCodePage { get; init; }

    /// <summary>
    /// Field delimiter (e.g. ",", ";", "\\t").
    /// </summary>
    public required string Delimiter { get; init; }

    public required int TimestampFieldIndex { get; init; }
    public required int TrackFieldIndex { get; init; }
    public required int ArtistFieldIndex { get; init; }

    public int AlbumFieldIndex { get; init; } = -1;
    public int AlbumArtistFieldIndex { get; init; } = -1;
    public int MillisecondsPlayedFieldIndex { get; init; } = -1;

    public bool FilterShortPlayedSongs { get; init; }
    public int MillisecondsPlayedThreshold { get; init; } = 30_000;

    /// <summary>
    /// Resolved text encoding.
    /// </summary>
    public Encoding Encoding => Encoding.GetEncoding(EncodingCodePage);

    /// <summary>
    /// Validates configuration consistency.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Delimiter))
            throw new InvalidOperationException("Delimiter must not be empty.");

        if (TimestampFieldIndex < 0)
            throw new InvalidOperationException("TimestampFieldIndex must be >= 0.");

        if (TrackFieldIndex < 0)
            throw new InvalidOperationException("TrackFieldIndex must be >= 0.");

        if (ArtistFieldIndex < 0)
            throw new InvalidOperationException("ArtistFieldIndex must be >= 0.");

        if (FilterShortPlayedSongs && MillisecondsPlayedThreshold <= 0)
            throw new InvalidOperationException("MillisecondsPlayedThreshold must be > 0.");
    }
}

