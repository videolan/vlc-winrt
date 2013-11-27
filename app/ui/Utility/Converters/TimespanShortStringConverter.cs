using System;
using Windows.UI.Xaml.Data;

namespace VLC_WINRT.Utility.Converters
{
    public class TimespanShortStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TimeSpan)
            {
                var ts = (TimeSpan) value;
                if (ts.Hours > 0)
                {

                    return String.Format("{0:hh\\:mm\\:ss}", ts);
                }
                else
                {
                    return String.Format("{0:mm\\:ss}", ts);
                }
            }
            else
            {
                return String.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}