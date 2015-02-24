/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Diagnostics;
using Windows.UI.Xaml.Data;

namespace VLC_WINRT_APP.Converters
{
    public class MillisecondsStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Int64)
            {
                var milliseconds = (Int64) value;
                if (milliseconds >= TimeSpan.MaxValue.TotalMilliseconds)
                {
                    //TODO: figure out what could cause this value to exceed MaxValue and cause
                    //an OverflowException in TimeSpan.FromMilliseconds
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                    return null;
                }

                TimeSpan time = TimeSpan.FromMilliseconds(milliseconds);
                if (time.Hours > 0)
                {
                    return String.Format("{0:hh\\:mm\\:ss}", time);
                }
                else
                {
                    return String.Format("{0:mm\\:ss}", time);
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
