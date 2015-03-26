using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using VLC_WINRT_APP.Model.Music;

namespace VLC_WINRT_APP.Converters
{
    public class MusicViewVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is MusicView)
            {
                var view = (MusicView) value;
                var viewName = view.ToString().ToLower();
                if (viewName == (string) parameter)
                    return Visibility.Visible;
                if (value is string && (string)value == "4" && (string)parameter == "4")
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
