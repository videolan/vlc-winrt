using System;
using System.Collections.Generic;
using System.Text;

namespace VLC_WinRT.Helpers
{
    public static class DeviceTypeHelper
    {
        public static DeviceTypeEnum GetDeviceType()
        {
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
