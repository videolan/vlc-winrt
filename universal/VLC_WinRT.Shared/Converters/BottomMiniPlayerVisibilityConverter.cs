using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using VLC_WinRT.Model;

namespace VLC_WinRT.Converters
{
    public class BottomMiniPlayerVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is VLCPage && (VLCPage) value != VLCPage.CurrentPlaylistPage && (VLCPage) value != VLCPage.MusicPlayerPage && (VLCPage)value != VLCPage.VideoPlayerPage)
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
