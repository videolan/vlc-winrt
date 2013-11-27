using System;
using Windows.UI.Xaml.Data;

namespace VLC_WINRT.Utility.Converters
{
    public class SecondsStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            TimeSpan time = TimeSpan.FromSeconds((double)value);
            if (time.Hours > 0)
            {

                return String.Format("{0:hh\\:mm\\:ss}", time);
            }
            else
            {
                return String.Format("{0:mm\\:ss}", time);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}