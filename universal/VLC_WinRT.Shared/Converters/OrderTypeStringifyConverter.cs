using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;
using VLC_WinRT.Model;

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
                    return resourceLoader.GetString("OrderByArtist");
                if ((OrderType)value == OrderType.ByDate)
                    return resourceLoader.GetString("OrderByDate");
            }
            else if (value is OrderListing)
            {
                if ((OrderListing) value == OrderListing.Ascending)
                    return resourceLoader.GetString("OrderAscending");
                if ((OrderListing) value == OrderListing.Descending)
                    return resourceLoader.GetString("OrderDescending");
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
