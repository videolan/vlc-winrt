using System;
using Windows.UI.Xaml.Data;
using VLC_WINRT_APP.Model.Video;

namespace VLC_WINRT_APP.Converters
{
    public class VideoViewStringifyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is VideoView)
            {
                switch ((VideoView)value)
                {
                    case VideoView.Videos:
                        return "videos";
                        break;
                    case VideoView.Shows:
                        return "shows";
                        break;
                    case VideoView.CameraRoll:
                        return "camera roll";
                        break;
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
