using System;
using Windows.UI.Xaml.Data;

namespace VLC_WINRT.Utility.Converters
{
    public class StringToUpperConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (string)value != null ? ((string)value).ToUpper() : "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
