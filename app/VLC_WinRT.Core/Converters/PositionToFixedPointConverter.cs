using System;
using Windows.UI.Xaml.Data;

namespace VLC_WinRT.Converters
{
    public class PositionToFixedPointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var floatingPoint = (Single)value;
            if (float.IsNaN(floatingPoint))
                return 0;
            var point = floatingPoint * 500;
            return (int)point;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var fixedPoint = (double)value;
            return (float)(fixedPoint / 500.0f);
        }
    }
}
