using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;
using VLC_WinRT.Model.Video;
using VLC_WinRT.ViewModels.VideoVM;

namespace VLC_WinRT.Converters
{
    public class ElapsedTimeToPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var item = value as VideoItem;
            if (item != null
                && item.Duration != null
                && item.Duration.TotalSeconds != 0
                && item.TimeWatched != TimeSpan.Zero)
            {
                double result = ((value as VideoItem).TimeWatched.TotalSeconds / item.Duration.TotalSeconds) * 100;
                return (int)result + "%";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
