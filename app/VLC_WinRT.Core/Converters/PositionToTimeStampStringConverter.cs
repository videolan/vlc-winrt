using System;
using Windows.UI.Xaml.Data;
using VLC.Model;
using VLC.ViewModels;

namespace VLC.Converters
{
    public class PositionToTimeStampStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var maxPos = 500;
            var pos = 0.0;
            var totalMs = 0.0;

            pos = Locator.MediaPlaybackViewModel.Position * maxPos;
            totalMs = Locator.MediaPlaybackViewModel.TimeTotal.TotalMilliseconds;

            var milliSeconds = (long)((pos / maxPos) * totalMs);
            return new MillisecondsStringConverter().Convert(milliSeconds, null, null, null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
