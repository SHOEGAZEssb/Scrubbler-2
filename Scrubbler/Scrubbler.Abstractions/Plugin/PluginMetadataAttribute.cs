namespace Scrubbler.Abstractions.Plugin;

[AttributeUsage(AttributeTargets.Class)]
public class PluginMetadataAttribute : Attribute
{
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public PlatformSupport SupportedPlatforms { get; init; } = PlatformSupport.All;
}

