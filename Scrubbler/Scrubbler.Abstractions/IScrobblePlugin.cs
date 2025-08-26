using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrubbler.Abstractions;
public interface IScrobblePlugin : IPlugin
{
    Task<IEnumerable<ScrobbleData>> GetScrobblesAsync(CancellationToken ct);
}
