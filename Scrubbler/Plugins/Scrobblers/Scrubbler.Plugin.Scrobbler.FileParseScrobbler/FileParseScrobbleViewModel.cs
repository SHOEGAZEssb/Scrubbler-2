using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Plugin;

namespace Scrubbler.Plugin.Scrobbler.FileParseScrobbler;

internal enum ScrobbleMode
{
    Import,
    UseScrobbleTimestamp
}

internal sealed partial class FileParseScrobbleViewModel : ScrobbleMultipleTimeViewModelBase<ParsedScrobbleViewModel>
{
    public override Task<IEnumerable<ScrobbleData>> GetScrobblesAsync()
    {
        throw new NotImplementedException();
    }
}
