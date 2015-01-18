using System;
using System.Collections.Generic;
using System.Text;

namespace VLC_WINRT_APP
{
    public static class Extensions
    {
        public static bool Contains(this string source, string value, StringComparison comparisonType)
        {
            return source.IndexOf(value, comparisonType) >= 0;
        }
    }
}
