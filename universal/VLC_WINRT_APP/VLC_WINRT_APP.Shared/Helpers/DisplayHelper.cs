
using Windows.Graphics.Display;

namespace VLC_WINRT_APP.Helpers
{
    public static class DisplayHelper
    {
        public static bool IsPortrait()
        {
            return DisplayProperties.CurrentOrientation != DisplayOrientations.Landscape && DisplayProperties.CurrentOrientation != DisplayOrientations.LandscapeFlipped;
        }
    }
}
