using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using VLC_WINRT_APP.Views.MusicPages;

namespace VLC_WINRT_APP.Converters
{
    public class BottomMiniPlayerVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
#if WINDOWS_PHONE_APP
            if (value is Type && (Type) value != typeof (MusicPlayerPage))
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
#else
            return Visibility.Visible;
#endif
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
