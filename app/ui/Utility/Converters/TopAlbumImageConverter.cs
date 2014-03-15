using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using VLC_WINRT.Model;

namespace VLC_WINRT.Utility.Converters
{
    public class TopAlbumImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var topImage = value as TopImage[];
            if (topImage == null) return null;
            return topImage.LastOrDefault(url => !string.IsNullOrEmpty(url.Text)).Text;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
