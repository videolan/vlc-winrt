
using Windows.Graphics.Display;
using Windows.System.Display;

namespace VLC_WinRT.Helpers
{
    public static class DisplayHelper
    {
        private static readonly DisplayRequest _displayAlwaysOnRequest = new DisplayRequest();
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
}
