using System;
using Windows.UI.Xaml.Data;

namespace VLC_WinRT.Converters
{
    public class OpacityDecreaserConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double val;
            if (value is double)
                val = (double)value;
            else if (value is int)
                val = (int)value;
            else val = 0;
            return 0.7 - val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
