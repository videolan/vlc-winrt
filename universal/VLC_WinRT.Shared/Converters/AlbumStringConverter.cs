using System;
using VLC_WinRT.Utils;
using Windows.UI.Xaml.Data;

namespace VLC_WinRT.Converters
{
    public class AlbumStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Strings.HumanizedAlbumName((string) value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
