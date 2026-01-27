namespace Scrubbler.Host.Models;

public record UserConfig
{
    public string? AccountFunctionsPluginID { get; set; }

    public bool CheckForUpdatesOnStartup { get; set; } = false;
}
