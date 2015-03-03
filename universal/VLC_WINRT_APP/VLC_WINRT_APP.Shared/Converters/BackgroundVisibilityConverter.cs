using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using VLC_WINRT_APP.Model;

namespace VLC_WINRT_APP.Converters
{
    public class BackgroundVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is PlayingType && (PlayingType)value == PlayingType.Video)
            {
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
