using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;

namespace VLC_WINRT_APP.Helpers
{
#if WINDOWS_PHONE_APP
    public static class StatusBarHelper
    {
        private static string title = "VLC Beta";
        public static void Default()
        {
            Set(App.Current.Resources["MainColor"] as SolidColorBrush, null, 1, title, ApplicationViewBoundsMode.UseVisible);
        }

        public static void SetTransparent()
        {
            Set(null, null, 0, title, ApplicationViewBoundsMode.UseCoreWindow);
        }

        public static void SetForeground(SolidColorBrush color)
        {
            StatusBar sB = StatusBar.GetForCurrentView();
            if (color != null) sB.ForegroundColor = color.Color;
        }

        static void Set(SolidColorBrush background, SolidColorBrush foreground, double opacity, string text, ApplicationViewBoundsMode boundsMode)
        {
            StatusBar sB = StatusBar.GetForCurrentView();
            if (background != null) 
                sB.BackgroundColor = background.Color;
            if (foreground != null)
                sB.ForegroundColor = foreground.Color;

            sB.BackgroundOpacity = opacity;
            var _ = sB.ProgressIndicator.ShowAsync();
            if (text != null) sB.ProgressIndicator.Text = text;
            var appView = ApplicationView.GetForCurrentView();
            appView.SetDesiredBoundsMode(boundsMode);

        }
    }
#endif
}