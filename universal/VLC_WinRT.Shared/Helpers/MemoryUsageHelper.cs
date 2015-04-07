
/* Unmerged change from project 'VLC_WinRT.Windows'
Before:
using System;
using System.Diagnostics;
using Windows.System;

namespace VLC_WinRT.Helpers
After:
namespace VLC_WinRT.Helpers
*/

using System;
using Windows.System;

namespace VLC_WinRT.Helpers
{
    public static class MemoryUsageHelper
    {
#if WINDOWS_PHONE_APP
        public static ulong MaxRamForResourceIntensiveTasks
        {
            get
            {
                ulong max = 0;
                try
                {
                    max = MemoryManager.AppMemoryUsageLimit;
                }
                catch
                {
                    
                }
                if (max < 200)
                    return 70;
                if (max < 400)
                    return 80;
                else return 85;
            }
        }

        public static ulong PercentMemoryUsed()
        {
            try
            {
                // Gets the app's current memory usage    
                ulong AppMemoryUsageUlong = MemoryManager.AppMemoryUsage;
                // Gets the app's memory usage limit    
                ulong AppMemoryUsageLimitUlong = MemoryManager.AppMemoryUsageLimit;

                AppMemoryUsageUlong /= 1024*1024;
                AppMemoryUsageLimitUlong /= 1024*1024;

                var level = (AppMemoryUsageUlong*100/AppMemoryUsageLimitUlong);
                LogHelper.Log(string.Format("UsedRAM:{0}-{2}%", AppMemoryUsageUlong, AppMemoryUsageLimitUlong, level));
                return level;
            }
            catch (InvalidCastException exception)
            {
                return 0;
            }
        }
#else
        // Just mock it on WinRT
        public static ulong MaxRamForResourceIntensiveTasks { get { return 100; } }
        public static ulong PercentMemoryUsed()
        {
            return 0;
        }
#endif
    }
}
