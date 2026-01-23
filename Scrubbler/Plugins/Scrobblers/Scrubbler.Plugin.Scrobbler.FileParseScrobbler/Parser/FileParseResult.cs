using System.Globalization;
using Scrubbler.Abstractions;

namespace Scrubbler.Plugin.Scrobbler.FileParseScrobbler.Parser;

internal sealed class FileParseResult(IEnumerable<ScrobbleData> scrobbles, IEnumerable<string> errors)
{
    #region Properties

    public IEnumerable<ScrobbleData> Scrobbles { get; } = scrobbles;

    public IEnumerable<string> Errors { get; } = errors;

    /// <summary>
    /// Different formats to try in case TryParse fails.
    /// </summary>
    private static readonly string[] _dateFormats = ["M/dd/yyyy h:mm"];

    #endregion Properties

    /// <summary>
    /// Tries to parse a string to a DateTime.
    /// </summary>
    /// <param name="dateString">String to parse.</param>
    /// <param name="dateTime">Outgoing DateTime.</param>
    /// <returns>True if <paramref name="dateString"/> was successfully parsed,
    /// otherwise false.</returns>
    public static bool TryParseDateString(string dateString, out DateTime dateTime)
    {
        if (!DateTime.TryParse(dateString, out dateTime))
        {
            bool parsed;
            // try different formats until succeeded
            foreach (string format in _dateFormats)
            {
                parsed = DateTime.TryParseExact(dateString, format, CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime);
                if (parsed)
                    return true;
            }

            return false;
        }

        return true;
    }
}
