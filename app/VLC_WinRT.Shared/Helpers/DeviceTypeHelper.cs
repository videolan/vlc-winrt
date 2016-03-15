using System;
using System.Collections.Generic;
using System.Text;

namespace VLC_WinRT.Helpers
{
    public static class DeviceTypeHelper
    {
        public static DeviceTypeEnum GetDeviceType()
        {
#if WINDOWS_UWP
            switch (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily)
            {
                case "Windows.Desktop":
                    return DeviceTypeEnum.Tablet;
                case "Windows.Mobile":
                    return DeviceTypeEnum.Phone;
                case "Windows.Universal":
                    return DeviceTypeEnum.IoT;
                case "Windows.Team":
                    return DeviceTypeEnum.SurfaceHub;
                case "Windows.Xbox":
                    return DeviceTypeEnum.Xbox;
                default:
                    return DeviceTypeEnum.Other;
            }
#elif WINDOWS_PHONE_APP
            return DeviceTypeEnum.Phone;
#elif WINDOWS_APP
            return DeviceTypeEnum.Tablet;
#endif
        }
    }

    public enum DeviceTypeEnum
    {
        Phone,
        Tablet,
        IoT,
        Xbox,
        SurfaceHub,
        Other
    }
}
