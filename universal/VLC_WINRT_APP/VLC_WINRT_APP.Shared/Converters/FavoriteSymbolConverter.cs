using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace VLC_WINRT_APP.Converters
{
    public class FavoriteSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool && (bool)value)
            {
                return new SymbolIcon(Symbol.UnFavorite);
            }
            return new SymbolIcon(Symbol.Favorite);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
