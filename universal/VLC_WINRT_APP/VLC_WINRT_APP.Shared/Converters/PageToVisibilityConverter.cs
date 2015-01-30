using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace VLC_WINRT_APP.Converters
{
    public class PageToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var currentpage = value as Type;
            var pageparameter = parameter.ToString();
            if (currentpage != null && currentpage.Name == pageparameter)
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
