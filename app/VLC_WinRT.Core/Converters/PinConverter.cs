using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Converters
{
    public class PinConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is bool)) return null;
            switch ((string)parameter)
            {
                case "text":
                    if ((bool)value)
                    {
                        return "unpin";
                    }
                    else
                    {
                        return "pin";
                    }
                case "icon":
                    var icon = ((bool)value) ? "UnpinSymbol" : "PinSymbol";
                    return new FontIcon()
                    {
                        FontFamily = new FontFamily(Strings.ModernFont),
                        Glyph = (string) App.Current.Resources[icon]
                    };
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
