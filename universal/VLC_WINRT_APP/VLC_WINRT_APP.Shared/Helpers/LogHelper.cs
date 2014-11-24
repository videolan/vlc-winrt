using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace VLC_WINRT_APP.Helpers
{
    public static class LogHelper
    {
        public static void Log(object o)
        {
            Debug.WriteLine(o.ToString());
        }
    }
}
