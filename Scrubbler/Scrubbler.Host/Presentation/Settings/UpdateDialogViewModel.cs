using System;
using System.Collections.Generic;
using System.Text;
using Scrubbler.Host.Updates;

namespace Scrubbler.Host.Presentation.Settings;

public class UpdateDialogViewModel(UpdateInfo info) : ObservableObject
{
    #region Properties

    public Version NewVersion => info.Version;

    public string Notes => info.Notes ?? "No update information provided";

    #endregion Properties
}
