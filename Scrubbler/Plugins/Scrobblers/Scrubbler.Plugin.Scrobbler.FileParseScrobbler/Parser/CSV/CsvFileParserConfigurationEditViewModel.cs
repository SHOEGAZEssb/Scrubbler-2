using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Scrubbler.Plugin.Scrobbler.FileParseScrobbler.Parser.CSV;

/// <summary>
/// Editable ViewModel for CSV file parser configuration.
/// </summary>
internal sealed partial class CsvFileParserConfigurationEditViewModel : ObservableObject
{
    #region Properties

    [ObservableProperty]
    private int _encodingCodePage = Encoding.UTF8.CodePage;

    /// <summary>
    /// Field delimiter as entered by the user (",", ";", "\t").
    /// </summary>
    [ObservableProperty]
    private string _delimiter = ";";

    [ObservableProperty]
    private int _timestampFieldIndex;

    [ObservableProperty]
    private int _trackFieldIndex = 1;

    [ObservableProperty]
    private int _artistFieldIndex = 2;

    [ObservableProperty]
    private int _albumFieldIndex = -1;

    [ObservableProperty]
    private int _albumArtistFieldIndex = -1;

    [ObservableProperty]
    private int _millisecondsPlayedFieldIndex = -1;

    [ObservableProperty]
    private bool _filterShortPlayedSongs;

    [ObservableProperty]
    private int _millisecondsPlayedThreshold = 30_000;

    #endregion Properties

    #region Construction

    public CsvFileParserConfigurationEditViewModel(CsvFileParserConfiguration config)
    {
        Delimiter = config.Delimiter;
        TimestampFieldIndex = config.TimestampFieldIndex;
        TrackFieldIndex = config.TrackFieldIndex;
        ArtistFieldIndex = config.ArtistFieldIndex;
        AlbumFieldIndex = config.AlbumFieldIndex;
        AlbumArtistFieldIndex = config.AlbumArtistFieldIndex;
        MillisecondsPlayedFieldIndex = config.MillisecondsPlayedFieldIndex;
        MillisecondsPlayedThreshold = config.MillisecondsPlayedThreshold;
        FilterShortPlayedSongs = config.FilterShortPlayedSongs;
    }

    #endregion Construction

    #region Conversion

    /// <summary>
    /// Creates an immutable parser configuration snapshot.
    /// </summary>
    public CsvFileParserConfiguration ToConfiguration()
    {
        var config = new CsvFileParserConfiguration
        {
            EncodingCodePage = EncodingCodePage,
            Delimiter = ResolveDelimiter(Delimiter),
            TimestampFieldIndex = TimestampFieldIndex,
            TrackFieldIndex = TrackFieldIndex,
            ArtistFieldIndex = ArtistFieldIndex,
            AlbumFieldIndex = AlbumFieldIndex,
            AlbumArtistFieldIndex = AlbumArtistFieldIndex,
            MillisecondsPlayedFieldIndex = MillisecondsPlayedFieldIndex,
            FilterShortPlayedSongs = FilterShortPlayedSongs,
            MillisecondsPlayedThreshold = MillisecondsPlayedThreshold
        };

        config.Validate();
        return config;
    }

    #endregion Conversion

    #region Helpers

    private static string ResolveDelimiter(string delimiter)
    {
        if (string.IsNullOrWhiteSpace(delimiter))
            return delimiter;

        return delimiter switch
        {
            "\\t" or "tab" or "TAB" => "\t",
            "\\n" => "\n",
            "\\r" => "\r",
            _ => delimiter
        };
    }

    #endregion Helpers
}

