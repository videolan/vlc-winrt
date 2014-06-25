/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using Windows.UI.Xaml.Data;

namespace VLC_WINRT_APP.Converters
{
    public class SecondsStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (double.IsNaN((double) value))
            {
                return "";
            }
            TimeSpan time = TimeSpan.FromSeconds((double)value);
            if (time.Hours > 0)
            {

                return String.Format("{0:hh\\:mm\\:ss}", time);
            }
            else
            {
                return String.Format("{0:mm\\:ss}", time);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
