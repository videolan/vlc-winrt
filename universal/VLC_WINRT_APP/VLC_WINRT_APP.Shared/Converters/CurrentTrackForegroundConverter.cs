using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.Converters
{
    public class CurrentTrackForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is MusicLibraryVM.TrackItem
                && Locator.MusicPlayerVM.CurrentTrackItem != null
                && ((MusicLibraryVM.TrackItem)value).Path == Locator.MusicPlayerVM.CurrentTrackItem.Path)
            {
                return App.Current.Resources["MainColor"] as SolidColorBrush;
            }
            return new SolidColorBrush(Color.FromArgb(255, 15, 15, 15));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
