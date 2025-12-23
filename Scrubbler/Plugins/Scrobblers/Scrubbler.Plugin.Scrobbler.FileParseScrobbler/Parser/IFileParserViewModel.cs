using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Scrubbler.Plugin.Scrobbler.FileParseScrobbler.Parser;

internal interface IFileParserViewModel<T> : INotifyPropertyChanged where T : IFileParserConfiguration
{
    string Name { get; }

    T Config { get; }

    IRelayCommand OpenSettingsCommand { get; }

    FileParseResult Parse(string file);
}
