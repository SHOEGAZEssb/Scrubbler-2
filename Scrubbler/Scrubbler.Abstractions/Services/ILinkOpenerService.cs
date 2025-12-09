namespace Scrubbler.Abstractions.Services;

public interface ILinkOpenerService
{
    Task OpenLink(string url);
}
