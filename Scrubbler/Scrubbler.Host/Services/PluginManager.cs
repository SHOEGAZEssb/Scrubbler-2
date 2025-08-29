using System.Reflection;
using System.Text.Json;
using Scrubbler.Abstractions.Logging;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Settings;
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

    public List<IPlugin> InstalledPlugins { get; private set; } = [];

    public List<PluginManifestEntry> AvailablePlugins { get; private set; } = [];

    public async Task InstallAsync(PluginManifestEntry plugin)
    {
        //var pluginDir = Path.Combine(AppContext.BaseDirectory, "Plugins");
        //Directory.CreateDirectory(pluginDir);

        //var filePath = Path.Combine(pluginDir, $"{plugin.Id}-{plugin.Version}.dll");
        //using var http = new HttpClient();
        //var data = await http.GetByteArrayAsync(plugin.SourceUri);
        //await File.WriteAllBytesAsync(filePath, data);

        //// load immediately
        //DiscoverInstalledPlugins();
    }

    public Task UninstallAsync(IPlugin plugin)
    {
        //InstalledPlugins.Remove(plugin);
        return Task.CompletedTask;
    }

    private void DiscoverInstalledPlugins()
    {
        var pluginDir = Path.Combine(AppContext.BaseDirectory, "Plugins");
        if (!Directory.Exists(pluginDir))
            Directory.CreateDirectory(pluginDir);

        foreach (var file in Directory.GetFiles(pluginDir, "*.dll"))
        {
            try
            {
                var asm = Assembly.LoadFrom(file);
                var plugins = asm.GetTypes()
                    .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract);

                foreach (var pluginType in plugins)
                {
                    if (Activator.CreateInstance(pluginType) is IPlugin plugin)
                    {
                        plugin.LogService = new ModuleLogService(_hostLogService, plugin.Name);
                        InstalledPlugins.Add(plugin);
                        _logService.Info($"Loaded Plugin: {plugin.Name} v{asm.GetName().Version}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.Error($"Failed to load plugin from {file}: {ex.Message}");
            }
        }
    }

    public async Task RefreshAvailablePluginsAsync()
    {
        IsFetchingPlugins = true;

        try
        {
            var repos = await _settings.GetAsync<List<PluginRepository>>("PluginRepositories")
                       ?? new List<PluginRepository>
                       {
                       // default fallback if settings.json has nothing
                       new("Default", "https://raw.githubusercontent.com/your-org/scrubbler-plugins/main/plugins.json")
                       };

            var all = new List<PluginManifestEntry>();

            foreach (var repo in repos)
            {
                try
                {
                    using var http = new HttpClient();
                    var json = await http.GetStringAsync(repo.Url);

                    var docs = JsonSerializer.Deserialize<List<PluginMetadata>>(json, new JsonSerializerOptions
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

            //AvailablePlugins = all;
        }
        finally
        {
            IsFetchingPlugins = false;
        }
    }
}
