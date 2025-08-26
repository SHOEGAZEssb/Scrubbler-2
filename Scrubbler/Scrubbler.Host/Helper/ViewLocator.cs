namespace Scrubbler.Host;

internal static class ViewLocator
{
    public static FrameworkElement Resolve(object viewModel)
    {
        if (viewModel == null)
            return new TextBlock { Text = "No ViewModel provided" };

        var vmType = viewModel.GetType();

        // Convention: FooViewModel -> FooView
        var viewTypeName = vmType.FullName!.Replace("ViewModel", "View");

        // Try to get the type from the same assembly as the ViewModel
        var viewType = vmType.Assembly.GetType(viewTypeName);

        if (viewType != null && Activator.CreateInstance(viewType) is FrameworkElement view)
        {
            view.DataContext = viewModel;
            return view;
        }

        return new TextBlock { Text = $"No view found for {vmType.Name}" };
    }
}
