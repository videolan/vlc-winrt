using System;
using System.Linq;
using VLC_WinRT.Model;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace VLC_WinRT.Converters
{
    public class PageToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var currentpage = (VLCPage)value;
            var pages = parameter.ToString().Split(',');
            if (pages.Contains(currentpage.ToString()))
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
