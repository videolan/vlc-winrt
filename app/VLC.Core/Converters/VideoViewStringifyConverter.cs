using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;
using VLC.Model.Video;
using VLC.Utils;

namespace VLC.Converters
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
                        return Strings.Videos.ToUpperFirstChar();
                    case VideoView.Shows:
                        return Strings.Shows.ToUpperFirstChar();
                    case VideoView.CameraRoll:
                        return Strings.CameraRoll.ToUpperFirstChar();
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
