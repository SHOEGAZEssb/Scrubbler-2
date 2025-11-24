using Microsoft.UI.Xaml.Data; // Updated namespace for WinUI

namespace Scrubbler.Abstractions.Converter
{
    public class BoolToTextConverter : IValueConverter
    {
        public string TrueText { get; set; } = "True";
        public string FalseText { get; set; } = "False";

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool booleanValue)
            {
                return booleanValue ? TrueText : FalseText;
            }

            return FalseText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException("BoolToTextConverter does not support ConvertBack.");
        }
    }
}
