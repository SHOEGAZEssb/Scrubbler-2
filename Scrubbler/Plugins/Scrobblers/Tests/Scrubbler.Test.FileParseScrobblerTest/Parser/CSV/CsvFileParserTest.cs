using System.Text;
using Scrubbler.Plugin.Scrobbler.FileParseScrobbler;
using Scrubbler.Plugin.Scrobbler.FileParseScrobbler.Parser;
using Scrubbler.Plugin.Scrobbler.FileParseScrobbler.Parser.CSV;

namespace Scrubbler.Test.FileParseScrobblerTest.Parser.CSV;

[TestFixture]
internal sealed class CsvFileParserTests
{
    private static string CreateTempCsv(string content, Encoding? encoding = null)
    {
        var path = Path.Combine(Path.GetTempPath(), $"scrubbler_csv_{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, content, encoding ?? new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        return path;
    }

    private static CsvFileParserConfiguration CreateConfig(
        string delimiter = ",",
        int encodingCodePage = 65001, // utf-8
        bool filterShort = false,
        int thresholdMs = 0,
        int timestampIndex = 0,
        int trackIndex = 1,
        int artistIndex = 2,
        int albumIndex = 3,
        int albumArtistIndex = 4,
        int msPlayedIndex = 5)
    {
        return new CsvFileParserConfiguration
        {
            Delimiter = delimiter,
            EncodingCodePage = encodingCodePage,

            TimestampFieldIndex = timestampIndex,
            TrackFieldIndex = trackIndex,
            ArtistFieldIndex = artistIndex,

            AlbumFieldIndex = albumIndex,
            AlbumArtistFieldIndex = albumArtistIndex,
            MillisecondsPlayedFieldIndex = msPlayedIndex,

            FilterShortPlayedSongs = filterShort,
            MillisecondsPlayedThreshold = thresholdMs,
        };
    }

    [Test]
    public void Parse_fileIsNull_throws()
    {
        var sut = new CsvFileParser();
        var config = CreateConfig();

        Assert.Throws<ArgumentNullException>(() => sut.Parse(null!, config, ScrobbleMode.UseScrobbleTimestamp));
    }

    [Test]
    public void Parse_fileIsEmpty_throws()
    {
        var sut = new CsvFileParser();
        var config = CreateConfig();

        Assert.Throws<ArgumentException>(() => sut.Parse(string.Empty, config, ScrobbleMode.UseScrobbleTimestamp));
    }

    [Test]
    public void Parse_useScrobbleTimestamp_validTimestamp_parses_and_addsOneSecond()
    {
        var sut = new CsvFileParser();
        var config = CreateConfig();

        var timestampString = "2025-01-02 03:04:05";
        Assert.That(FileParseResult.TryParseDateString(timestampString, out var parsed), Is.True,
            "Test assumption failed: FileParseResult.TryParseDateString could not parse the timestampString. Change the format to one your app supports.");

        var csv =
            $"{timestampString},Track B,Artist B,Album B,AlbumArtist B,00:10:00\n";

        var file = CreateTempCsv(csv, config.Encoding);
        try
        {
            var result = sut.Parse(file, config, ScrobbleMode.UseScrobbleTimestamp);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Errors, Is.Empty);
                Assert.That(result.Scrobbles.Count(), Is.EqualTo(1));
            }

            var s = result.Scrobbles.Single();
            Assert.That(s.Timestamp.DateTime, Is.EqualTo(parsed.AddSeconds(1)));
        }
        finally
        {
            File.Delete(file);
        }
    }

    [Test]
    public void Parse_useScrobbleTimestamp_blankTimestamp_addsError_and_skipsRow()
    {
        var sut = new CsvFileParser();
        var config = CreateConfig();

        var csv =
            ",Track C,Artist C,Album C,AlbumArtist C,00:10:00\n";

        var file = CreateTempCsv(csv, config.Encoding);
        try
        {
            var result = sut.Parse(file, config, ScrobbleMode.UseScrobbleTimestamp);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Scrobbles, Is.Empty);
                Assert.That(result.Errors.Count(), Is.EqualTo(1));
                Assert.That(result.Errors.Single(), Does.StartWith("CSV line 1:"));
            }
            Assert.That(result.Errors.Single(), Does.Contain("Timestamp could not be parsed"));
        }
        finally
        {
            File.Delete(file);
        }
    }

    [Test]
    public void Parse_useScrobbleTimestamp_invalidTimestamp_addsError_and_skipsRow()
    {
        var sut = new CsvFileParser();
        var config = CreateConfig();

        var csv =
            "definitely-not-a-date,Track D,Artist D,Album D,AlbumArtist D,00:10:00\n";

        var file = CreateTempCsv(csv, config.Encoding);
        try
        {
            var result = sut.Parse(file, config, ScrobbleMode.UseScrobbleTimestamp);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Scrobbles, Is.Empty);
                Assert.That(result.Errors.Count(), Is.EqualTo(1));
                Assert.That(result.Errors.Single(), Does.StartWith("CSV line 1:"));
            }
            Assert.That(result.Errors.Single(), Does.Contain("Timestamp could not be parsed"));
        }
        finally
        {
            File.Delete(file);
        }
    }

    [Test]
    public void Parse_shortPlayFilter_enabled_skipsRow_whenPlayedMillisecondsIsBelowOrEqualThreshold()
    {
        var sut = new CsvFileParser();

        // threshold is ms; parser does TimeSpan.TryParse on row.MillisecondsPlayed and compares TotalMilliseconds
        // 00:00:01 -> 1000ms
        var config = CreateConfig(filterShort: true, thresholdMs: 1000);

        var csv =
            "2025-01-02 03:04:05,Track E,Artist E,Album E,AlbumArtist E,00:00:01\n";

        var file = CreateTempCsv(csv, config.Encoding);
        try
        {
            var result = sut.Parse(file, config, ScrobbleMode.UseScrobbleTimestamp);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Errors, Is.Empty);
                Assert.That(result.Scrobbles, Is.Empty);
            }
        }
        finally
        {
            File.Delete(file);
        }
    }

    [Test]
    public void Parse_shortPlayFilter_enabled_doesNotSkipRow_whenPlayedMillisecondsIsAboveThreshold()
    {
        var sut = new CsvFileParser();

        // 00:00:02 -> 2000ms
        var config = CreateConfig(filterShort: true, thresholdMs: 1000);

        var csv =
            "2025-01-02 03:04:05,Track F,Artist F,Album F,AlbumArtist F,00:00:02\n";

        var file = CreateTempCsv(csv, config.Encoding);
        try
        {
            var result = sut.Parse(file, config, ScrobbleMode.UseScrobbleTimestamp);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Errors, Is.Empty);
                Assert.That(result.Scrobbles.Count(), Is.EqualTo(1));
            }
        }
        finally
        {
            File.Delete(file);
        }
    }

    [Test]
    public void Parse_shortPlayFilter_enabled_doesNotSkipRow_whenPlayedFieldIsNotATimeSpan()
    {
        var sut = new CsvFileParser();
        var config = CreateConfig(filterShort: true, thresholdMs: 1000);

        var csv =
            "2025-01-02 03:04:05,Track G,Artist G,Album G,AlbumArtist G,not-a-timespan\n";

        var file = CreateTempCsv(csv, config.Encoding);
        try
        {
            var result = sut.Parse(file, config, ScrobbleMode.UseScrobbleTimestamp);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Errors, Is.Empty);
                Assert.That(result.Scrobbles.Count(), Is.EqualTo(1));
            }
        }
        finally
        {
            File.Delete(file);
        }
    }

    [Test]
    public void Parse_usesDelimiter_fromConfig()
    {
        var sut = new CsvFileParser();

        var config = CreateConfig(delimiter: ";");

        var csv =
            "2025-01-02 03:04:05;Track H;Artist H;Album H;AlbumArtist H;00:00:10\n";

        var file = CreateTempCsv(csv, config.Encoding);
        try
        {
            var result = sut.Parse(file, config, ScrobbleMode.UseScrobbleTimestamp);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Errors, Is.Empty);
                Assert.That(result.Scrobbles.Count(), Is.EqualTo(1));
            }

            var s = result.Scrobbles.Single();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(s.Track, Is.EqualTo("Track H"));
                Assert.That(s.Artist, Is.EqualTo("Artist H"));
            }
        }
        finally
        {
            File.Delete(file);
        }
    }

    [Test]
    public void Parse_multipleRows_reportsCorrectLineNumber_forSecondRowError()
    {
        var sut = new CsvFileParser();
        var config = CreateConfig();

        var validTimestamp = "2025-01-02 03:04:05";
        Assert.That(FileParseResult.TryParseDateString(validTimestamp, out _), Is.True,
            "Test assumption failed: adjust validTimestamp to a supported format.");

        var csv =
            $"{validTimestamp},Track I,Artist I,Album I,AlbumArtist I,00:00:10\n" +
            $"bad-date,Track J,Artist J,Album J,AlbumArtist J,00:00:10\n";

        var file = CreateTempCsv(csv, config.Encoding);
        try
        {
            var result = sut.Parse(file, config, ScrobbleMode.UseScrobbleTimestamp);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Scrobbles.Count(), Is.EqualTo(1));
                Assert.That(result.Errors.Count(), Is.EqualTo(1));
                Assert.That(result.Errors.Single(), Does.StartWith("CSV line 2:"));
            }
        }
        finally
        {
            File.Delete(file);
        }
    }
}
