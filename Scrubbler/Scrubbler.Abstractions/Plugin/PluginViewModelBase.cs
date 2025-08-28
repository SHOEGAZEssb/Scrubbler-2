using CommunityToolkit.Mvvm.ComponentModel;
using Scrubbler.Abstractions.Plugin;

namespace Scrubbler.Abstractions;
public abstract partial class PluginViewModelBase : ObservableObject, IPluginViewModel
{
    /// <summary>
    /// Indicates whether the plugin is currently busy (e.g. loading, processing).
    /// </summary>
    [ObservableProperty]
    protected bool _isBusy;

    /// <summary>
    /// Stores the latest error message, if any.
    /// </summary>
    [ObservableProperty]
    protected string? _errorMessage;
}
