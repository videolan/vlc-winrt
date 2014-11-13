
using Windows.Graphics.Display;

namespace VLC_WINRT_APP.Helpers
{
    public static class DisplayHelper
    {
        public static bool IsPortrait()
        {
            return DisplayInformation.GetForCurrentView().CurrentOrientation != DisplayOrientations.Landscape &&
                DisplayInformation.GetForCurrentView().CurrentOrientation != DisplayOrientations.LandscapeFlipped;
        }
    }
}
