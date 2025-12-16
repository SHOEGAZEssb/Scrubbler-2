using System.Collections.ObjectModel;
using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Plugin;

namespace Scrubbler.Tests.AbstractionsTest.Plugin;

internal sealed class TestScrobbleMultipleViewModel
    : ScrobbleMultipleViewModelBase<IScrobbableObjectViewModel>
{
    public override Task<IEnumerable<ScrobbleData>> GetScrobblesAsync()
        => Task.FromResult<IEnumerable<ScrobbleData>>(Array.Empty<ScrobbleData>());

    public void SetScrobbles(IEnumerable<IScrobbableObjectViewModel> scrobbles)
    {
        Scrobbles = new ObservableCollection<IScrobbableObjectViewModel>(scrobbles);
    }
}
