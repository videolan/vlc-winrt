using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;
using VLC_WinRT.Model.Video;

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
                        return resourceLoader.GetString("Videos").ToLower();
                    case VideoView.Shows:
                        return resourceLoader.GetString("Shows").ToLower();
                    case VideoView.CameraRoll:
                        return resourceLoader.GetString("CameraRoll/Text").ToLower();
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
