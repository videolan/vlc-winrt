
using System.Diagnostics;
using Windows.System;

namespace VLC_WINRT_APP.Helpers
{
    public static class MemoryUsageHelper
    {
#if WINDOWS_PHONE_APP
        public static ulong MaxRamForResourceIntensiveTasks
        {
            get
            {
                var max = MemoryManager.AppMemoryUsageLimit;
                if (max < 200)
                    return 70;
                if (max < 400)
                    return 80;
                else return 85;
            }
        }

        public static ulong PercentMemoryUsed()
        {
            // Gets the app's current memory usage    
            ulong AppMemoryUsageUlong = MemoryManager.AppMemoryUsage;
            // Gets the app's memory usage limit    
            ulong AppMemoryUsageLimitUlong = MemoryManager.AppMemoryUsageLimit;

            AppMemoryUsageUlong /= 1024 * 1024;
            AppMemoryUsageLimitUlong /= 1024 * 1024;

            var level = (AppMemoryUsageUlong * 100 / AppMemoryUsageLimitUlong);
            LogHelper.Log(string.Format("UsedRAM:{0}-{2}%", AppMemoryUsageUlong, AppMemoryUsageLimitUlong, level));
            return level;
        }
#else
        // Just mock it on WindowsRT
        public static ulong MaxRamForResourceIntensiveTasks { get { return 100; } }
        public static ulong PercentMemoryUsed()
        {
            return 0;
        }
#endif
    }
}
