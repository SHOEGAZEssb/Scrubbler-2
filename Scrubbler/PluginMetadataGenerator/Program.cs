using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Scrubbler.Abstractions.Plugin;

record PluginManifestEntry(
    string Id,
    string Name,
    string Version,
    string Description,
    Uri? IconUri,
    string PluginType,
    IReadOnlyList<string> SupportedPlatforms,
    Uri SourceUri
);

class Program
{
    // map of plugin marker interfaces → human-friendly type labels
    private static readonly Dictionary<Type, string> _pluginTypes = new()
    {
        { typeof(IAccountPlugin), "Account Plugin" },
        { typeof(IScrobblePlugin), "Scrobble Plugin" }
        // add more here as you introduce new plugin kinds
    };

    static async Task Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: PluginMetadataGenerator <pluginDir> <outputJson> <baseDownloadUrl>");
            Environment.Exit(1);
        }

        var pluginDir = args[0];
        var outputPath = args[1];
        var baseUrl = args[2].TrimEnd('/');

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        var entries = new List<PluginManifestEntry>();

        foreach (var dll in Directory.GetFiles(pluginDir, "Scrubbler.Plugin.*.dll"))
        {
            try
            {
                var asm = Assembly.LoadFrom(dll);

                // find all IPlugin implementations
                var pluginTypes = asm.GetTypes()
                    .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract);

                foreach (var type in pluginTypes)
                {
                    if (Activator.CreateInstance(type) is not IPlugin plugin)
                    {
                        Console.WriteLine($"Skipping {dll}:{type.Name} → could not instantiate");
                        continue;
                    }

                    var id = type.FullName?.ToLowerInvariant() ?? Path.GetFileNameWithoutExtension(dll).ToLowerInvariant();
                    var rawVersion = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                                     ?? asm.GetName().Version?.ToString()
                                     ?? "0.0.0";

                    // drop build metadata (e.g. +sha) and pre-release info if you want
                    var version = rawVersion.Split('+')[0];

                    // resolve type label dynamically
                    var pluginTypeLabel = ResolvePluginType(type);

                    var entry = new PluginManifestEntry(
                        Id: id,
                        Name: plugin.Name,
                        Version: version,
                        Description: plugin.Description,
                        IconUri: null,
                        PluginType: pluginTypeLabel,
                        SupportedPlatforms: plugin.SupportedPlatforms.ToString().Split(", "),
                        SourceUri: new Uri($"{baseUrl}/{Path.GetFileName(dll)}")
                    );

                    entries.Add(entry);
                    Console.WriteLine($"Added {plugin.Name} v{version} ({pluginTypeLabel})");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to inspect {dll}: {ex.Message}");
            }
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(entries, options));
        Console.WriteLine($"Wrote {entries.Count} entries to {outputPath}");
    }

    private static string ResolvePluginType(Type pluginType)
    {
        foreach (var kvp in _pluginTypes)
        {
            if (kvp.Key.IsAssignableFrom(pluginType))
                return kvp.Value;
        }
        return "Plugin"; // fallback
    }
}
