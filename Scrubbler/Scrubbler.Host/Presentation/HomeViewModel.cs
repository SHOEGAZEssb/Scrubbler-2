using CommunityToolkit.Mvvm.Input;
using Scrubbler.Host.Updates;

namespace Scrubbler.Host.Presentation;
internal partial class HomeViewModel : ObservableObject
{
    [RelayCommand]
    private async Task Update()
    {
        var http = new HttpClient();
        var source = new JsonManifestUpdateSource(new Uri("http://localhost:8000/manifest.json"), http);

        var updateManager = new UpdateManager(http, source);

        // check
        var update = await updateManager.CheckForUpdateAsync(CancellationToken.None);
        if (update is null)
            return;

        // apply (will exit the app if it starts the updater)
        await updateManager.ApplyUpdateAndRestartAsync(update, CancellationToken.None);

    }
}
