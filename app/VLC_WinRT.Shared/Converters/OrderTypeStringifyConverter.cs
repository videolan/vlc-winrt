using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;
using VLC_WinRT.Model;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Converters
{
    public class OrderTypeStringifyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var resourceLoader = new ResourceLoader();
            if (value is OrderType)
            {
                if ((OrderType)value == OrderType.ByArtist)
                    return Strings.OrderByArtist;
                if ((OrderType)value == OrderType.ByDate)
                    return Strings.OrderByDate;
                if ((OrderType)value == OrderType.ByAlbum)
                    return Strings.OrderByAlbum;
            }
            else if (value is OrderListing)
            {
                if ((OrderListing) value == OrderListing.Ascending)
                    return Strings.OrderAscending;
                if ((OrderListing) value == OrderListing.Descending)
                    return Strings.OrderDescending;
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
