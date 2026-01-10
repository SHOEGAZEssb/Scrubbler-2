using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Services;
using Scrubbler.Plugin.Scrobbler.FileParseScrobbler.Parser;
using Scrubbler.Plugin.Scrobbler.FileParseScrobbler.Parser.CSV;

namespace Scrubbler.Plugin.Scrobbler.FileParseScrobbler;

internal enum ScrobbleMode
{
    Import,
    UseScrobbleTimestamp
}

internal sealed partial class FileParseScrobbleViewModel : ScrobbleMultipleTimeViewModelBase<ParsedScrobbleViewModel>
{
    #region Properties

    [ObservableProperty]
    private IFileParserViewModel _selectedParser;

    [ObservableProperty]
    private IEnumerable<IFileParserViewModel> _availableParsers;

    [ObservableProperty]
    private ScrobbleMode _selectedScrobbleMode = ScrobbleMode.Import;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ParseCommand))]
    private string _selectedFilePath = string.Empty;

    private bool CanParse => !string.IsNullOrEmpty(SelectedFilePath) && File.Exists(SelectedFilePath);

    private readonly ILogService _logService;
    private readonly IDialogService _dialogService;
    private readonly IFilePickerService _filePicker;
    private readonly IFileStorageService _fileStorageService;
    private static readonly string[] _textFiles = [".txt"];

    #endregion Properties

    #region Construction

    public FileParseScrobbleViewModel(ILogService logService, IDialogService dialogService, IFilePickerService filePicker, IFileStorageService fileStorageService,
                                      IFileParser<CsvFileParserConfiguration> csvParser, CsvFileParserConfiguration csvConfig)
    {
        _logService = logService;
        _dialogService = dialogService;
        _filePicker = filePicker;
        _fileStorageService = fileStorageService;

        var parsers = new List<IFileParserViewModel>
        {
            new CsvFileParserViewModel(dialogService, csvParser, csvConfig)
        };

        AvailableParsers = parsers;
        SelectedParser = parsers[0];
    }

    #endregion Construction

    public override Task<IEnumerable<ScrobbleData>> GetScrobblesAsync()
    {
        throw new NotImplementedException();
    }

    [RelayCommand]
    private async Task OpenFile()
    {
        var file = await _filePicker.PickFileAsync(SelectedParser.SupportedExtensions);
        if (file == null)
            return;

        SelectedFilePath = file.Path;
    }

    [RelayCommand(CanExecute = nameof(CanParse))]
    private async Task Parse()
    {
        IsBusy = true;
        try
        {
            FileParseResult result = null!;
            await Task.Run(() =>
            {
                result = SelectedParser.Parse(SelectedFilePath, SelectedScrobbleMode);
            });

            if (result.Errors.Any())
            {
                var dialog = new ContentDialog
                {
                    Title = "Parsing Errors",
                    Content = "There were errors during parsing. Do you want to save a file containing the error information?",
                    PrimaryButtonText = "Yes",
                    SecondaryButtonText = "No",
                    DefaultButton = ContentDialogButton.Primary
                };

                var res = await _dialogService.ShowDialogAsync(dialog);
                if (res == ContentDialogResult.Primary)
                {
                    var file = await _filePicker.SaveFileAsync(
                                                "scrobbles",
                                                new Dictionary<string, IReadOnlyList<string>>
                                                {
                                                    { "Text file", _textFiles }
                                                });
                    if (file != null)
                    {
                        await _fileStorageService.WriteLinesAsync(file, result.Errors);
                    }
                }
            }

            Scrobbles.Clear();
            foreach (var scrobble in result.Scrobbles)
            {
                Scrobbles.Add(new ParsedScrobbleViewModel(scrobble));
            }
        }
        catch (Exception ex)
        {
            _logService.Error("An error occurred while parsing the selected file.", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
