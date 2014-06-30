using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.ViewModels;

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
