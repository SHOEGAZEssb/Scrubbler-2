using CommunityToolkit.Mvvm.Input;

namespace Scrubbler.Plugin.Scrobbler.FileParseScrobbler.Parser;

internal interface IConfigurableFileParserViewModel<T> : IFileParserViewModel where T : IFileParserConfiguration
{
    T Config { get; }

    IAsyncRelayCommand OpenSettingsCommand { get; }
}
