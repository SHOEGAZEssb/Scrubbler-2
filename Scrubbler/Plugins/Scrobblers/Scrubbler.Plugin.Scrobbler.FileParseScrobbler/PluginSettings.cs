using Scrubbler.Abstractions.Settings;
using Scrubbler.Plugin.Scrobbler.FileParseScrobbler.Parser.CSV;

namespace Scrubbler.Plugin.Scrobbler.FileParseScrobbler;

internal sealed class PluginSettings : IPluginSettings
{
    public CsvFileParserConfiguration CsvConfig { get; set; } = CsvFileParserConfiguration.Default;
}
