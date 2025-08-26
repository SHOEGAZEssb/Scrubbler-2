using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrubbler.Abstractions;
public abstract class ScrobblePluginViewModelBase : PluginViewModelBase
{
    public abstract IEnumerable<ScrobbleData> GetScrobbles();
}
