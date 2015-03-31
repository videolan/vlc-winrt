using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using VLC_WinRT.Helpers;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Converters
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
