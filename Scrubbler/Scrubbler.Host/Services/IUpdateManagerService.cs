using Scrubbler.Host.Updates;

namespace Scrubbler.Host.Services;

public interface IUpdateManagerService
{
    /// <summary>
    /// Checks whether an update is available.
    /// </summary>
    Task<UpdateInfo?> CheckForUpdateAsync(CancellationToken ct);

    /// <summary>
    /// Downloads + verifies + launches updater, then exits the app.
    /// </summary>
    Task ApplyUpdateAndRestartAsync(UpdateInfo update, CancellationToken ct);
}
