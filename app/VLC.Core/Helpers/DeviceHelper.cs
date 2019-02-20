using Windows.Graphics.Display;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Display;

namespace VLC.Helpers
{
    public static class DeviceHelper
    {
        private static readonly DisplayRequest _displayAlwaysOnRequest = new DisplayRequest();
        static EasClientDeviceInformation _deviceInfo = new EasClientDeviceInformation();

        public static DeviceTypeEnum GetDeviceType()
        {
            if(_deviceInfo.OperatingSystem.Equals("WindowsPhone"))
                return DeviceTypeEnum.Phone;
            else return DeviceTypeEnum.Tablet;
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
