using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT_APP.ViewModels.VideoVM;

namespace VLC_WINRT_APP.Converters
{
    public class ElapsedTimeToPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is MediaViewModel
                && (value as MediaViewModel).Duration != null
                && (value as MediaViewModel).Duration.TotalSeconds != 0)
            {
                double result = ((value as MediaViewModel).TimeWatched.TotalSeconds / (value as MediaViewModel).Duration.TotalSeconds) * 100;
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
