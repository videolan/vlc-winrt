using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using VLC_WinRT.Utils;

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

        public static string ExceptionToString(Exception exception)
        {
            bool inner = false;
            var strBuilder = new StringBuilder();
            strBuilder.AppendLine(Strings.MemoryUsage());

            for (Exception ex = exception; ex != null; ex = ex.InnerException)
            {
                strBuilder.AppendLine(inner ? "InnerException" : "Exception:");
                strBuilder.AppendLine("Message: ");
                strBuilder.AppendLine(ex.Message);
                strBuilder.AppendLine("HelpLink: ");
                strBuilder.AppendLine(ex.HelpLink);
                strBuilder.AppendLine("HResult: ");
                strBuilder.AppendLine(ex.HResult.ToString());
                strBuilder.AppendLine("Source: ");
                strBuilder.AppendLine(ex.Source);
                strBuilder.AppendLine("StackTrace: ");
                strBuilder.AppendLine(ex.StackTrace);
                strBuilder.AppendLine("");

                if (exception.Data != null && exception.Data.Count > 0)
                {
                    strBuilder.AppendLine("Additional Data: ");
                    foreach (DictionaryEntry entry in exception.Data)
                    {
                        strBuilder.AppendLine(entry.Key + ";" + entry.Value);
                    }
                    strBuilder.AppendLine("");
                }

                inner = true;
            }
            return strBuilder.ToString();
        }
    }
}
