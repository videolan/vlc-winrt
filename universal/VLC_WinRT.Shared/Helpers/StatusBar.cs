#if WINDOWS_PHONE_APP
using System;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;
using System.Threading.Tasks;

namespace VLC_WinRT.Helpers
{
    public static class StatusBarHelper
    {
        public static void Default()
        {
            Set(App.Current.Resources["StatusBarColor"] as SolidColorBrush, null, 1, "", ApplicationViewBoundsMode.UseVisible);
        }

        public static void SetTransparent()
        {
            Set(null, App.Current.Resources["MainColor"] as SolidColorBrush, 0, "");
        }

        public static void SetForeground(SolidColorBrush color)
        {
            StatusBar sB = StatusBar.GetForCurrentView();
            if (color != null) sB.ForegroundColor = color.Color;
        }

        static void Set(SolidColorBrush background, SolidColorBrush foreground, double opacity, string text, ApplicationViewBoundsMode? boundsMode = ApplicationViewBoundsMode.UseVisible, double? progress = 0)
        {
            StatusBar sB = StatusBar.GetForCurrentView();
            if (background != null)
                sB.BackgroundColor = background.Color;
            if (foreground != null)
                sB.ForegroundColor = foreground.Color;

            sB.BackgroundOpacity = opacity;
            if (!string.IsNullOrEmpty(text))
            {
                var _ = sB.ProgressIndicator.ShowAsync();
                if (text != null) sB.ProgressIndicator.Text = text;
                var appView = ApplicationView.GetForCurrentView();
                if (boundsMode != null && boundsMode.HasValue)
                    appView.SetDesiredBoundsMode(boundsMode.Value);
                sB.ProgressIndicator.ProgressValue = progress;
            }
            else
            {
                var _ = sB.ProgressIndicator.HideAsync();
            }
        }

        public static void SetDefaultForPage(Type type)
        {
            Default();
        }

        public static Rect OccludedRect
        {
            get
            {
                StatusBar sB = StatusBar.GetForCurrentView();
                return sB.OccludedRect;
            }
        }

        public static async Task Hide()
        {
            StatusBar sB = StatusBar.GetForCurrentView();
            await sB.HideAsync();
        }

        public static async Task Show()
        {
            StatusBar sB = StatusBar.GetForCurrentView();
            await sB.ShowAsync();
        }
    }
}
#endif