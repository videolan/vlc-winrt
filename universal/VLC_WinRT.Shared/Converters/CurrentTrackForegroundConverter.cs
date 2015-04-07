/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Converters
{
    public class CurrentTrackForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TrackItem
                && Locator.MusicPlayerVM.CurrentTrack != null
                && ((TrackItem)value).Path == Locator.MusicPlayerVM.CurrentTrack.Path)
            {
                return App.Current.Resources["MainColor"] as SolidColorBrush;
            }

            if (!string.IsNullOrEmpty((string)parameter) && (string)parameter == "Dark")
            {
                return new SolidColorBrush(Colors.WhiteSmoke);
            }
            return new SolidColorBrush(Color.FromArgb(255, 15, 15, 15));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
