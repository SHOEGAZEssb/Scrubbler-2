namespace Scrubbler.Host.Helper;

internal static class ViewLocator
{
    /// <summary>
    /// Resolves a view for the given view model using naming conventions.
    /// </summary>
    /// <remarks>
    /// Uses the convention that FooViewModel maps to FooView. Searches in the same assembly as the view model.
    /// </remarks>
    /// <param name="viewModel">The view model instance for which to resolve a view.</param>
    /// <returns>A <see cref="FrameworkElement"/> representing the view for the view model. Returns a <see cref="TextBlock"/> with an error message if no view is found.</returns>
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
