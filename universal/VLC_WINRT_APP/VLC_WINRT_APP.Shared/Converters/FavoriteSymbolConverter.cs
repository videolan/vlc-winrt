using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Utils;

namespace VLC_WINRT_APP.Converters
{
    public class FavoriteSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool && (bool)value)
            {
                return new FontIcon()
                {
                    FontFamily = new FontFamily(Strings.ModernFont),
                    Glyph = (string) App.Current.Resources["UnfavoriteSymbol"]
                };
            }
            return new FontIcon()
            {
                FontFamily = new FontFamily(Strings.ModernFont),
                Glyph = (string)App.Current.Resources["FavoriteSymbol"]
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
