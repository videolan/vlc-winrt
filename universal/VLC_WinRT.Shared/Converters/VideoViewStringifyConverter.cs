using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;
using VLC_WinRT.Model.Video;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Converters
{
    public class VideoViewStringifyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var resourceLoader = new ResourceLoader();
            if (value is VideoView)
            {
                switch ((VideoView)value)
                {
                    case VideoView.Videos:
                        return Strings.Videos.ToUpper();
                    case VideoView.Shows:
                        return Strings.Shows.ToUpper();
                    case VideoView.CameraRoll:
                        return Strings.CameraRoll.ToUpper();
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
