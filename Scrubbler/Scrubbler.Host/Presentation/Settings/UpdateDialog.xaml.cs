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
using Scrubbler.Host.Presentation.Plugins;
using Scrubbler.Host.Updates;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Scrubbler.Host.Presentation.Settings;

public sealed partial class UpdateDialog : ContentDialog
{
    public UpdateDialogViewModel ViewModel { get; }

    public UpdateDialog(UpdateInfo info)
    {
        this.InitializeComponent();
        ViewModel = new UpdateDialogViewModel(info);
    }
}
