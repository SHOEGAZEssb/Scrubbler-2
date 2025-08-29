using System.Reflection;
using System.Text.Json;
using Scrubbler.Abstractions.Logging;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Settings;
using Scrubbler.Host.Helper;
using Scrubbler.Host.Services.Logging;

namespace Scrubbler.Host.Services;

internal class PluginManager : IPluginManager
{
    private readonly HostLogService _hostLogService;
    private readonly ILogService _logService;
    private readonly ISettingsStore _settings;

    public bool IsFetchingPlugins
    {
        get => _isFetchingPlugins;
        private set
        {
            if (IsFetchingPlugins != value)
            {
                _isFetchingPlugins = value;
                IsFetchingPluginsChanged?.Invoke(this, IsFetchingPlugins);
            }
        }
    }
    private bool _isFetchingPlugins;

    public event EventHandler<bool>? IsFetchingPluginsChanged;

    public PluginManager(HostLogService hostLogService, ISettingsStore settings)
    {
        _hostLogService = hostLogService;
        _settings = settings;
        _logService = new ModuleLogService(_hostLogService, "Plugin Manager");
        DiscoverInstalledPlugins();
        _ = RefreshAvailablePluginsAsync();
    }

    public IEnumerable<IPlugin> InstalledPlugins => _installed.Select(p => p.Plugin);
    private readonly List<(IPlugin Plugin, PluginLoadContext Context)> _installed = [];

    public List<PluginManifestEntry> AvailablePlugins { get; private set; } = [];

    public async Task InstallAsync(PluginManifestEntry plugin)
    {
        var rootDir = Path.Combine(AppContext.BaseDirectory, "Plugins");
        Directory.CreateDirectory(rootDir);

        // subfolder per plugin version to avoid clashes
        var pluginDir = Path.Combine(rootDir, $"{plugin.Id}");
        if (Directory.Exists(pluginDir))
            Directory.Delete(pluginDir, recursive: true);
        Directory.CreateDirectory(pluginDir);

        // download zip
        var zipPath = Path.Combine(pluginDir, $"{plugin.Id}.zip");
        using var http = new HttpClient();
        var data = await http.GetByteArrayAsync(plugin.SourceUri);
        await File.WriteAllBytesAsync(zipPath, data);

        // extract zip into the pluginDir
        System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, pluginDir);

        // optionally remove the zip after extraction
        File.Delete(zipPath);

        // load immediately (point DiscoverInstalledPlugins at pluginDir)
        DiscoverInstalledPlugins();
    }

    public async Task UninstallAsync(IPlugin plugin)
    {
        // find tuple
        var entry = _installed.FirstOrDefault(x => x.Plugin == plugin);
        if (entry.Plugin == null)
            return;

        _installed.Remove(entry);

        // unload
        entry.Context.Unload();
        _logService.Info($"Unloaded plugin: {plugin.Name}");

        // force GC to reclaim
        await Task.Run(() =>
        {
            for (int i = 0; i < 3; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        });

        try
        {
            // find containing directory of the plugin DLL
            var pluginAssembly = entry.Plugin.GetType().Assembly.Location;
            var pluginFolder = Path.GetDirectoryName(pluginAssembly);
            if (!string.IsNullOrEmpty(pluginFolder) && Directory.Exists(pluginFolder))
            {
                Directory.Delete(pluginFolder, recursive: true);
                _logService.Info($"Deleted plugin files from {pluginFolder}");
            }
        }
        catch (Exception ex)
        {
            _logService.Error($"Failed to remove plugin files: {ex.Message}");
        }
    }

    private void DiscoverInstalledPlugins()
    {
        var rootDir = Path.Combine(AppContext.BaseDirectory, "Plugins");
        if (!Directory.Exists(rootDir))
            Directory.CreateDirectory(rootDir);

        foreach (var dll in Directory.EnumerateFiles(rootDir, "Scrubbler.Plugin.*.dll", SearchOption.AllDirectories))
        {
            try
            {
                var context = new PluginLoadContext(dll);
                var asm = context.LoadFromAssemblyPath(dll);

                var pluginTypes = asm.GetTypes();
                var pluginTypes2 = pluginTypes.Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract);


                foreach (var pluginType in pluginTypes)
                {
                    if (Activator.CreateInstance(pluginType) is IPlugin plugin)
                    {
                        plugin.LogService = new ModuleLogService(_hostLogService, plugin.Name);
                        _installed.Add((plugin, context));
                        _logService.Info($"Loaded Plugin: {plugin.Name} v{asm.GetName().Version}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.Error($"Failed to load plugin from {dll}: {ex.Message}");
            }
        }
    }

    public async Task RefreshAvailablePluginsAsync()
    {
        AvailablePlugins.Clear();
        IsFetchingPlugins = true;

        try
        {
            var repos = await _settings.GetAsync<List<PluginRepository>>("PluginRepositories")
                       ??
                       [
                       // default fallback if settings.json has nothing
                       new("Default", "https://raw.githubusercontent.com/shoegazessb/scrubbler-plugins/main/plugins.json")
                       ];

            var all = new List<PluginManifestEntry>();

            using var http = new HttpClient();
            foreach (var repo in repos)
            {
                try
                {
                    var json = await http.GetStringAsync(repo.Url);

                    var docs = JsonSerializer.Deserialize<List<PluginManifestEntry>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (docs != null)
                    {
                        all.AddRange(docs);
                        _logService.Info($"Fetched {docs.Count} plugins from repo {repo.Name}");
                    }
                }
                catch (Exception ex)
                {
                    _logService.Error($"Failed to fetch plugins from repo {repo.Name}: {ex.Message}");
                }
            }

            AvailablePlugins = all;
        }
        finally
        {
            IsFetchingPlugins = false;
        }
    }
}
