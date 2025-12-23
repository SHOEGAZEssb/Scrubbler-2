using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Scrubbler.Abstractions.Services;

namespace Scrubbler.Plugin.Scrobbler.FileParseScrobbler.Parser.CSV;

internal sealed partial class CsvFileParserViewModel(IDialogService dialogService, CsvFileParserConfiguration initialConfig) : ObservableObject, IFileParserViewModel<CsvFileParserConfiguration>
{
    #region Properties

    public string Name { get; } = "CSV";

    public CsvFileParserConfiguration Config { get; } = initialConfig;

    private readonly IDialogService _dialogService = dialogService;

    #endregion Properties

    [RelayCommand]
    private void OpenSettings()
    {

    }

    public FileParseResult Parse(string file)
    {
        throw new NotImplementedException();
    }
}
