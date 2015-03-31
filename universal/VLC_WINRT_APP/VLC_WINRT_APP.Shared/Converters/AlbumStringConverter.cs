using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace VLC_WinRT.Converters
{
    public class AlbumStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is String && !string.IsNullOrEmpty((string)value))
            {
                return value;
            }

            // This might be expensive, but it won't be called too often.
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            return loader.GetString("UnknownAlbum");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
