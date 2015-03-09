using System;
using Windows.UI.Xaml.Data;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Converters
{
    public class PositionToTimeStampStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var maxPos = 500;
            var pos = 0.0;
            var totalMs = 0.0;

            if (Locator.MediaPlaybackViewModel.PlayingType == PlayingType.Music)
            {
                pos = Locator.MediaPlaybackViewModel.Position * maxPos;
                totalMs = Locator.MediaPlaybackViewModel.TimeTotal.TotalMilliseconds;
            }
            else if (Locator.MediaPlaybackViewModel.PlayingType == PlayingType.Video)
            {
                pos = Locator.MediaPlaybackViewModel.Position * maxPos;
                totalMs = Locator.MediaPlaybackViewModel.TimeTotal.TotalMilliseconds;
            }
            var milliSeconds = (long)((pos / maxPos) * totalMs);
            return new MillisecondsStringConverter().Convert(milliSeconds, null, null, null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
