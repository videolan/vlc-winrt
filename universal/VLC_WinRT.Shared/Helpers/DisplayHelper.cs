
using Windows.Graphics.Display;

namespace VLC_WinRT.Helpers
{
    public static class DisplayHelper
    {
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
    }
}
