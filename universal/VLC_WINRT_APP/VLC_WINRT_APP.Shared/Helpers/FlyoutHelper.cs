using Windows.UI.Xaml;

namespace VLC_WINRT_APP.Helpers
{
    public static class FlyoutHelper
    {
        public static double GetSettingsFlyoutWidthFromWindowWidth(double max)
        {
            var width = Window.Current.Bounds.Width;
            return width < max ? width : max;
        }
    }
}
