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

namespace VLC_WINRT_APP.Utility.Converters
{
    public class TimespanShortStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TimeSpan)
            {
                var ts = (TimeSpan) value;
                if (ts.Hours > 0)
                {

                    return String.Format("{0:hh\\:mm\\:ss}", ts);
                }
                else
                {
                    return String.Format("{0:mm\\:ss}", ts);
                }
            }
            else
            {
                return String.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
