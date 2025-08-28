using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Scrubbler.Host.Services.Logging;

namespace Scrubbler.Host.Presentation.Logging;

internal class LogViewModel : ObservableObject
{
    public ObservableCollection<LogMessage> Entries { get; } = [];

    public LogViewModel(HostLogService hostLogService)
    {
        hostLogService.MessageLogged += Add;
    }

    private void Add(LogMessage entry)
    {
        Entries.Add(entry);
    }
}
