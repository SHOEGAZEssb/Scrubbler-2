using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrubbler.Abstractions;
public interface IAutoScrobblePlugin : IPlugin
{
    event EventHandler<IEnumerable<ScrobbleData>> ScrobblesDetected;
}
