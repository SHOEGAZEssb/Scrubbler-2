using CommunityToolkit.Mvvm.ComponentModel;
using Scrubbler.Abstractions.Plugin;

namespace Scrubbler.Abstractions;

/// <summary>
/// Base class for all plugin view models providing common functionality.
/// </summary>
/// <remarks>
/// Plugin implementations should inherit from this class rather than implementing <see cref="IPluginViewModel"/> directly.
/// </remarks>
/// <seealso cref="IPluginViewModel"/>
/// <seealso cref="ScrobblePluginViewModelBase"/>
public abstract partial class PluginViewModelBase : ObservableObject, IPluginViewModel
{
    /// <summary>
    /// Gets or sets a value indicating whether the plugin is currently busy (e.g. loading, processing).
    /// </summary>
    [ObservableProperty]
    protected bool _isBusy;
}
