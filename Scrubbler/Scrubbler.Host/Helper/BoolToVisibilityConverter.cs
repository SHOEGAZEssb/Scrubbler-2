using Microsoft.UI.Xaml.Data;

namespace Scrubbler.Host.Helper;

/// <summary>
/// Converts a boolean value to a <see cref="Visibility"/> value.
/// </summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets whether the conversion should be inverted.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, <c>true</c> maps to <see cref="Visibility.Collapsed"/> and <c>false</c> maps to <see cref="Visibility.Visible"/>.
    /// When <c>false</c> (default), <c>true</c> maps to <see cref="Visibility.Visible"/> and <c>false</c> maps to <see cref="Visibility.Collapsed"/>.
    /// </remarks>
    public bool Invert { get; set; } = false;

    /// <summary>
    /// Converts a boolean value to a <see cref="Visibility"/> value.
    /// </summary>
    /// <param name="value">The boolean value to convert.</param>
    /// <param name="targetType">The target type (unused).</param>
    /// <param name="parameter">Optional parameter (unused).</param>
    /// <param name="language">The language (unused).</param>
    /// <returns><see cref="Visibility.Visible"/> if the value is <c>true</c> (or inverted if <see cref="Invert"/> is <c>true</c>); otherwise, <see cref="Visibility.Collapsed"/>.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var b = value is bool v && v;
        if (Invert) b = !b;
        return b ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// Converts a <see cref="Visibility"/> value back to a boolean.
    /// </summary>
    /// <param name="value">The visibility value to convert.</param>
    /// <param name="targetType">The target type (unused).</param>
    /// <param name="parameter">Optional parameter (unused).</param>
    /// <param name="language">The language (unused).</param>
    /// <returns><c>true</c> if the visibility is <see cref="Visibility.Visible"/> (inverted if <see cref="Invert"/> is <c>true</c>); otherwise, <c>false</c>.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => (value is Visibility v && v == Visibility.Visible) ^ Invert;
}

