using System.ComponentModel;

namespace Scrubbler.Abstractions.Plugin;

/// <summary>
/// Base interface for all plugin view models.
/// </summary>
/// <remarks>
/// Plugins must provide a view model that implements this interface to enable UI interactions.
/// </remarks>
/// <seealso cref="PluginViewModelBase"/>
public interface IPluginViewModel : INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <summary>
    /// Gets a value indicating whether the plugin is currently busy performing an operation.
    /// </summary>
    bool IsBusy { get; }
}
