#if WINDOWS_PHONE_APP
using System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;
using VLC_WINRT_APP.Views.MainPages;
using VLC_WINRT_APP.Views.MusicPages.ArtistPages;

namespace VLC_WINRT_APP.Helpers
{
    public static class StatusBarHelper
    {
        public static void Default()
        {
            Set(App.Current.Resources["MainColor"] as SolidColorBrush, null, 1, "", ApplicationViewBoundsMode.UseVisible);
        }

        public static void SetTransparent()
        {
            Set(null, null, 0, "", ApplicationViewBoundsMode.UseCoreWindow);
        }

        public static void UpdateTitle(string t)
        {
            Set(null, null, 0, t, ApplicationViewBoundsMode.UseCoreWindow, null);
        }

        public static void SetForeground(SolidColorBrush color)
        {
            StatusBar sB = StatusBar.GetForCurrentView();
            if (color != null) sB.ForegroundColor = color.Color;
        }

        static void Set(SolidColorBrush background, SolidColorBrush foreground, double opacity, string text, ApplicationViewBoundsMode? boundsMode, double? progress = 0)
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
            SetTransparent();
        }
    }
}
#endif