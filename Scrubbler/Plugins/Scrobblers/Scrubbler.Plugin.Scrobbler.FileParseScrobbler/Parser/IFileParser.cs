namespace Scrubbler.Plugin.Scrobbler.FileParseScrobbler.Parser;

internal interface IFileParser<T> where T : IFileParserConfiguration
{
    FileParseResult Parse(string file, T config, ScrobbleMode mode);
}
