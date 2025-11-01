using Microsoft.UI.Xaml.Data;

namespace Scrubbler.Host.Helper;

/// <summary>
/// Converts a boolean value to its inverse (negation).
/// </summary>
public class InverseBooleanConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean value to its inverse.
    /// </summary>
    /// <param name="value">The boolean value to invert.</param>
    /// <param name="targetType">The target type (unused).</param>
    /// <param name="parameter">Optional parameter (unused).</param>
    /// <param name="language">The language (unused).</param>
    /// <returns>The inverted boolean value, or the original value if it is not a boolean.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is bool b ? !b : value;

    /// <summary>
    /// Converts a boolean value back to its inverse (same as Convert).
    /// </summary>
    /// <param name="value">The boolean value to invert.</param>
    /// <param name="targetType">The target type (unused).</param>
    /// <param name="parameter">Optional parameter (unused).</param>
    /// <param name="language">The language (unused).</param>
    /// <returns>The inverted boolean value, or the original value if it is not a boolean.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => value is bool b ? !b : value;
}
