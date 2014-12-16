using System;
using Windows.UI.Xaml.Data;
using Newtonsoft.Json.Schema;

namespace VLC_WINRT_APP.Converters
{
    public class NullIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int && (int) value == 0)
            {
                return "";
            }
            if (value is string && (string) value == "0")
            {
                return "";
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
