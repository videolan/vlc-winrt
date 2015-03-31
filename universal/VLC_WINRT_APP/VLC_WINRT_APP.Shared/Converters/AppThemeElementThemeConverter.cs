using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace VLC_WinRT.Converters
{
    public class AppThemeElementThemeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is ApplicationTheme && (ApplicationTheme) value == ApplicationTheme.Dark) ? ElementTheme.Dark : ElementTheme.Light;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
