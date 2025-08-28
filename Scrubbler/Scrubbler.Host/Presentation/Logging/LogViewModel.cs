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

    private readonly DispatcherQueue _dispatcher; // for thread-safe UI updates

    public LogViewModel(HostLogService hostLogService)
    {
        _dispatcher = DispatcherQueue.GetForCurrentThread();
        hostLogService.MessageLogged += Add;
    }

    private void Add(LogMessage entry)
    {
        // ensure UI thread
        if (_dispatcher.HasThreadAccess)
        {
            Entries.Add(entry);
        }
        else
        {
            _dispatcher.TryEnqueue(() => Entries.Add(entry));
        }
    }
}
