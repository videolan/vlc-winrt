using System;
using Windows.UI.Xaml.Data;

namespace VLC_WinRT.Converters
{
    public class FavoriteLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool && (bool)value)
            {
                return "unfavorite";
            }
            return "favorite";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
