using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using VLC_WINRT_APP.Views.MusicPages;
using VLC_WINRT_APP.Views.VideoPages;

namespace VLC_WINRT_APP.Converters
{
    public class BottomMiniPlayerVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Type && (Type) value != typeof (MusicPlayerPage) && (Type)value != typeof(VideoPlayerPage))
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
