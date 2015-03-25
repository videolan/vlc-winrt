using System;
using Windows.UI.Xaml.Data;

namespace VLC_WINRT_APP.Converters
{
    public class BooleanToMainColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((bool) value)
            {
                return App.Current.Resources["InactiveMainColor"];
            }
            return App.Current.Resources["MainColor"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
