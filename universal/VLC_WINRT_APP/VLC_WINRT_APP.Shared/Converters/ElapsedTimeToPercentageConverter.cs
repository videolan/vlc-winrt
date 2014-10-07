using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;
using VLC_WINRT_APP.Model.Video;
using VLC_WINRT_APP.ViewModels.VideoVM;

namespace VLC_WINRT_APP.Converters
{
    public class ElapsedTimeToPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is VideoVM
                && (value as VideoVM).Duration != null
                && (value as VideoVM).Duration.TotalSeconds != 0)
            {
                double result = ((value as VideoVM).TimeWatched.TotalSeconds / (value as VideoVM).Duration.TotalSeconds) * 100;
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
