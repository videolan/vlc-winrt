using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.Graphics.Display;
using Windows.System.Display;

namespace VLC.Helpers
{
    public static class DeviceHelper
    {
        private static readonly DisplayRequest _displayAlwaysOnRequest = new DisplayRequest();

        public static DeviceTypeEnum GetDeviceType()
        {
            return DeviceTypeEnum.Tablet;

            //switch (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily)
            //{
            //    case "Windows.Desktop":
            //        return DeviceTypeEnum.Tablet;
            //    case "Windows.Mobile":
            //        return DeviceTypeEnum.Phone;
            //    case "Windows.Universal":
            //        return DeviceTypeEnum.IoT;
            //    case "Windows.Team":
            //        return DeviceTypeEnum.SurfaceHub;
            //    case "Windows.Xbox":
            //        return DeviceTypeEnum.Xbox;
            //    default:
            //        return DeviceTypeEnum.Other;
            //}
        }

        public static bool IsMediaCenterModeCompliant => false;

        public static bool IsPortrait()
        {
            var o = DisplayInformation.GetForCurrentView().CurrentOrientation;
            switch (o)
            {
                case DisplayOrientations.Portrait:
                case DisplayOrientations.PortraitFlipped:
                case DisplayOrientations.None:
                    return true;
                case DisplayOrientations.Landscape:
                case DisplayOrientations.LandscapeFlipped:
                    return false;
                default:
                    return true;
            }
        }

        public static void PrivateDisplayCall(bool shouldActivate)
        {
            if (_displayAlwaysOnRequest == null) return;
            try
            {
                if (shouldActivate)
                {
                    _displayAlwaysOnRequest.RequestActive();
                }
                else
                {
                    _displayAlwaysOnRequest.RequestRelease();
                }
            }
            catch { }
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
