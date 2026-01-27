using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Scrubbler.Abstractions.Services;

namespace Scrubbler.Plugin.Scrobbler.FileParseScrobbler.Parser.CSV;

internal sealed partial class CsvFileParserViewModel(IDialogService dialogService, IFileParser<CsvFileParserConfiguration> parser, CsvFileParserConfiguration initialConfig) : ObservableObject, IConfigurableFileParserViewModel<CsvFileParserConfiguration>
{
    #region Properties

    public string Name { get; } = "CSV";

    public CsvFileParserConfiguration Config { get; private set; } = initialConfig;

    public IReadOnlyList<string> SupportedExtensions => [".csv"];

    private readonly IDialogService _dialogService = dialogService;
    private readonly IFileParser<CsvFileParserConfiguration> _parser = parser;

    #endregion Properties

    [RelayCommand]
    private async Task OpenSettings()
    {
        var vm = new CsvFileParserConfigurationEditViewModel(Config);
        var dialog = new CsvFileParserConfigurationEditView
        {
            DataContext = vm
        };

        var result = await _dialogService.ShowDialogAsync(dialog);
        if (result == ContentDialogResult.Primary)
            Config = vm.ToConfiguration();
    }

    public FileParseResult Parse(string file, ScrobbleMode mode)
    {
        return _parser.Parse(file, Config, mode);
    }
}
