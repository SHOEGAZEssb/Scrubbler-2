using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Services;
using Scrubbler.Abstractions.Settings;
using Scrubbler.Plugin.Scrobbler.FileParseScrobbler.Parser.CSV;

namespace Scrubbler.Plugin.Scrobbler.FileParseScrobbler;

[PluginMetadata(
    Name = "File Parse Scrobbler",
    Description = "Parses scrobbles from local files",
    SupportedPlatforms = PlatformSupport.All)]
public sealed class FileParseScrobblePlugin : Abstractions.Plugin.PluginBase, IScrobblePlugin, IPersistentPlugin
{
    #region Properties

    private readonly JsonSettingsStore _settingsStore;
    private PluginSettings _settings = new();
    private FileParseScrobbleViewModel? _vm;
    private readonly IDialogService _dialogService;
    private readonly IFilePickerService _filePickerService;
    private readonly IFileStorageService _fileStorageService;

    #endregion Properties

    #region Construction

    public FileParseScrobblePlugin(IModuleLogServiceFactory logFactory, IDialogService dialogService, IFilePickerService filePickerService, IFileStorageService fileStorageService)
        : base(logFactory)
    {
        var settingsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Scrubbler", "Plugins", Name);
        Directory.CreateDirectory(settingsDir);
        _settingsStore = new JsonSettingsStore(Path.Combine(settingsDir, "settings.json"));
        _dialogService = dialogService;
        _filePickerService = filePickerService;
        _fileStorageService = fileStorageService;
    }

    #endregion Construction

    public override IPluginViewModel GetViewModel()
    {
        _vm ??= new FileParseScrobbleViewModel(_logService, _dialogService, _filePickerService, _fileStorageService,
                                                new CsvFileParser(),
                                                _settings.CsvConfig);

        return _vm;
    }

    public async Task LoadAsync()
    {
        _logService.Debug("Loading settings...");

        _settings = await _settingsStore.GetOrCreateAsync<PluginSettings>(Name);
    }

    public async Task SaveAsync()
    {
        _logService.Debug("Saving settings...");
        var csvParser = _vm?.AvailableParsers.OfType<CsvFileParserViewModel>().FirstOrDefault();
        if (csvParser is not null)
            _settings.CsvConfig = csvParser.Config;

        await _settingsStore.SetAsync(Name, _settings);
    }
}
