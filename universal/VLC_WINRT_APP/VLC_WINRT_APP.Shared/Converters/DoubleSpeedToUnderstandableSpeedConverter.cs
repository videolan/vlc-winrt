using System;
using Windows.UI.Xaml.Data;

namespace VLC_WINRT_APP.Converters
{
    public class DoubleSpeedToUnderstandableSpeedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return "x" + (double)value / 100;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
