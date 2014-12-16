using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace VLC_WINRT_APP.Converters
{
    public class PositionToFixedPointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var floatingPoint = (Single)value;
            if (float.IsNaN(floatingPoint))
                return 0;
            return (int)(floatingPoint * 100.0f);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var fixedPoint = (double)value;
            return (float)(fixedPoint / 100.0f);
        }
    }
}
