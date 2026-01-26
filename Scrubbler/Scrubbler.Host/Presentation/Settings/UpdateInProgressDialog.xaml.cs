using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Scrubbler.Host.Updates;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Scrubbler.Host.Presentation.Settings;

public sealed partial class UpdateInProgressDialog : ContentDialog
{
    public UpdateInProgressDialog(UpdateManager manager, UpdateInfo info)
    {
        this.InitializeComponent();
        _ = RunUpdate(manager, info);
    }

    private async Task RunUpdate(UpdateManager manager, UpdateInfo info)
    {
        try
        {
            await manager.ApplyUpdateAndRestartAsync(info, CancellationToken.None);
        }
        catch(Exception ex)
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
