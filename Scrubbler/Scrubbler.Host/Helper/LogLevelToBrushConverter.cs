using Microsoft.UI;
using Microsoft.UI.Xaml.Data;

namespace Scrubbler.Host.Helper;

public class LogLevelToBrushConverter : IValueConverter
{
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

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}
