using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace VLC_WINRT_APP.Converters
{
    public class PinConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is bool)) return null;
            if ((string)parameter == "text")
            {
                if ((bool)value)
                {
                    return "unpin";
                }
                else
                {
                    return "pin";
                }
            }
            else if ((string)parameter == "icon")
            {
                if ((bool)value)
                {
                    return new SymbolIcon(Symbol.UnPin);
                }
                else
                {
                    return new SymbolIcon(Symbol.Pin);
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
