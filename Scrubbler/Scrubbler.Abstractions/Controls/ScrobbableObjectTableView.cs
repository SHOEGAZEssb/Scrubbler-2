using WinUI.TableView;

namespace Scrubbler.Abstractions.Controls;

public class ScrobbableObjectTableView : TableView
{
    public ScrobbableObjectTableView()
    {
        SelectionChanged += ScrobbableObjectTableView_SelectionChanged;
    }

    private void ScrobbableObjectTableView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        foreach (var item in e.AddedItems.OfType<IScrobbableObjectViewModel>().SkipLast(1))
            item.UpdateIsSelectedSilent(true);
        var lastAddedItem = e.AddedItems.OfType<IScrobbableObjectViewModel>().LastOrDefault();
        if (lastAddedItem != null)
            lastAddedItem.IsSelected = true;

        foreach (var item in e.RemovedItems.OfType<IScrobbableObjectViewModel>().SkipLast(1))
            item.UpdateIsSelectedSilent(false);
        var lastRemovedItem = e.RemovedItems.OfType<IScrobbableObjectViewModel>().LastOrDefault();
        if (lastRemovedItem != null)
            lastRemovedItem.IsSelected = false;
    }
}
