using Microsoft.UI;
using Microsoft.UI.Xaml.Data;

namespace Scrubbler.Host.Helper;

/// <summary>
/// Converts a <see cref="LogLevel"/> value to a <see cref="SolidColorBrush"/> for UI display.
/// </summary>
public class LogLevelToBrushConverter : IValueConverter
{
    /// <summary>
    /// Converts a <see cref="LogLevel"/> to a color brush.
    /// </summary>
    /// <param name="value">The log level to convert.</param>
    /// <param name="targetType">The target type (unused).</param>
    /// <param name="parameter">Optional parameter (unused).</param>
    /// <param name="language">The language (unused).</param>
    /// <returns>A <see cref="SolidColorBrush"/> corresponding to the log level, or white if the value is not a valid log level.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => new SolidColorBrush(Colors.Gray),
                LogLevel.Information => new SolidColorBrush(Colors.LightGreen),
                LogLevel.Warning => new SolidColorBrush(Colors.Orange),
                LogLevel.Error => new SolidColorBrush(Colors.Red),
                LogLevel.Critical => new SolidColorBrush(Colors.DarkRed),
                _ => new SolidColorBrush(Colors.White)
            };
        }
        return new SolidColorBrush(Colors.White);
    }

    /// <summary>
    /// Converts a brush back to a log level. Not implemented.
    /// </summary>
    /// <param name="value">The brush value.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">Optional parameter.</param>
    /// <param name="language">The language.</param>
    /// <returns>Throws <see cref="NotImplementedException"/>.</returns>
    /// <exception cref="NotImplementedException">This conversion is not supported.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}
