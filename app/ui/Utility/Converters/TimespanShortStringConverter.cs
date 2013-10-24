using System;
using Windows.UI.Xaml.Data;

namespace VLC_WINRT.Utility.Converters
{
    public class TimespanShortStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TimeSpan)
            {
                var ts = (TimeSpan) value;
                return ts.Hours + ":" + ts.Minutes +":" + ts.Seconds;
            }
            else
            {
                return String.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}