namespace Scrubbler.Host.Models;

public record AppConfig
{
    public string? Environment { get; init; }
}
