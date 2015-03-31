using Windows.Devices.Input;

namespace VLC_WinRT.Helpers
{
    public static class CapabilitiesHelper
    {
        public static bool IsTouchCapable
        {
            get
            {
                TouchCapabilities touchCap = new TouchCapabilities();
                return touchCap.TouchPresent > 0;
            }
        }
    }
}