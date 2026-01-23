using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Scrubbler.Abstractions;

namespace Scrubbler.Plugin.Scrobbler.FileParseScrobbler.Parser.CSV;

internal sealed class CsvFileParser : IFileParser<CsvFileParserConfiguration>
{
    public FileParseResult Parse(string file, CsvFileParserConfiguration config, ScrobbleMode mode)
    {
        ArgumentException.ThrowIfNullOrEmpty(file);

        var scrobbles = new List<ScrobbleData>();
        var errors = new List<string>();

        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            Delimiter = config.Delimiter,
            Encoding = config.Encoding,
            BadDataFound = null,
            MissingFieldFound = null,
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StreamReader(file, csvConfig.Encoding);
        using var csv = new CsvReader(reader, csvConfig);

        csv.Context.RegisterClassMap(new CsvScrobbleRowMap(config));

        var rowIndex = 0;
        foreach (var row in csv.GetRecords<CsvScrobbleRow>())
        {
            rowIndex++;

            try
            {
                // Timestamp handling
                DateTime playedAt = DateTime.Now;
                if (mode == ScrobbleMode.UseScrobbleTimestamp)
                {
                    if (string.IsNullOrWhiteSpace(row.Timestamp) || !FileParseResult.TryParseDateString(row.Timestamp, out playedAt))
                        throw new FormatException("Timestamp could not be parsed");
                }

                // Short-play filter
                if (config.FilterShortPlayedSongs && TimeSpan.TryParse(row.MillisecondsPlayed, out var played) && played.TotalMilliseconds <= config.MillisecondsPlayedThreshold)
                    continue;

                scrobbles.Add(
                    new ScrobbleData(row.Track, row.Artist, playedAt.AddSeconds(1))
                    {
                        Album = row.Album,
                        AlbumArtist = row.AlbumArtist
                    }
                );
            }
            catch (Exception ex)
            {
                errors.Add($"CSV line {rowIndex}: {ex.Message}");
            }
        }

        return new FileParseResult(scrobbles, errors);
    }
}
