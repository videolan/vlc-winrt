using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace VLC_WINRT_APP.Converters
{
    public class ApplicationThemeBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is ApplicationTheme && (ApplicationTheme) value == ApplicationTheme.Dark;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is bool && (bool)value)
            {
                return ApplicationTheme.Dark;
            }
            return ApplicationTheme.Light;
        }
    }
}
