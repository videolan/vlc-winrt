using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace VLC_WINRT.Utility.Converters
{
    class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int)
            {
                if ((int) value == 0)
                    return Visibility.Visible;
                    return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
