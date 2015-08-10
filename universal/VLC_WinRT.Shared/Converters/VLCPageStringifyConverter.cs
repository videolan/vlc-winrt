using System;
using Windows.UI.Xaml.Data;
using VLC_WinRT.Model;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Converters
{
    public class VLCPageStringifyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is VLCPage)
            {
                switch ((VLCPage)value)
                {
                    case VLCPage.MainPageHome:
                        return Strings.Home;
                    case VLCPage.MainPageMusic:
                        return Strings.Music;
                    case VLCPage.MainPageVideo:
                        return Strings.Videos;
                    case VLCPage.MainPageFileExplorer:
                        return Strings.FileExplorer;
                    default:
                        throw new NotImplementedException();
                }
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
