using Scrubbler.Host.Services;
using Scrubbler.Host.Updates;

namespace Scrubbler.Host.Presentation.Settings;

public sealed partial class UpdateInProgressDialog : ContentDialog
{
    public UpdateInProgressDialog(IUpdateManagerService manager, UpdateInfo info)
    {
        this.InitializeComponent();
        _ = RunUpdate(manager, info);
    }

    private async Task RunUpdate(IUpdateManagerService manager, UpdateInfo info)
    {
        try
        {
            await manager.ApplyUpdateAndRestartAsync(info, CancellationToken.None);
        }
        catch (Exception ex)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                ProgressStackPanel.Visibility = Visibility.Collapsed;
                ErrorStackPanel.Visibility = Visibility.Visible;
                ErrorText.Text = $"Update failed: {ex.Message}";
                IsPrimaryButtonEnabled = true;
            });
        }
    }
}
