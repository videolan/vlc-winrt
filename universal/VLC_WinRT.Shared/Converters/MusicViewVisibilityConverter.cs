using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using VLC_WinRT.Model.Music;

namespace VLC_WinRT.Converters
{
    public class MusicViewVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is MusicView)
            {
                var view = (MusicView) value;
                var viewName = (int)view;
                var allowedViews = ((string)parameter).Split(',');
                if (allowedViews.Contains(viewName.ToString()))
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
