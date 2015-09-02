using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace VLC_WinRT.Helpers
{
    public static class StringsHelper
    {
        public static string MillisecondsToString(long value)
        {
            var milliseconds = (Int64)value;
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

        public static string SecondsToString(double value)
        {
            long milliseconds = (long)value*1000;
            return MillisecondsToString(milliseconds);
        }
    }
}
