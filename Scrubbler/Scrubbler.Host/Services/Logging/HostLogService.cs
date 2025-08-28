using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrubbler.Host.Services.Logging;
internal class HostLogService
{
    public event Action<LogMessage>? MessageLogged;

    internal void Write(LogLevel level, string module, string message, Exception? ex = null)
    {
        var entry = new LogMessage(DateTime.UtcNow, level, module, message, ex);
        MessageLogged?.Invoke(entry);
    }
}
