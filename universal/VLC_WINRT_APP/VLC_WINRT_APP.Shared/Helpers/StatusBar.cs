using System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;
using VLC_WINRT_APP.Views.MainPages;
using VLC_WINRT_APP.Views.MusicPages.ArtistPages;

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

        public static void UpdateTitle(string t)
        {
            Set(App.Current.Resources["MainColor"] as SolidColorBrush, null, 1, t, ApplicationViewBoundsMode.UseVisible, null);
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
            var _ = sB.ProgressIndicator.ShowAsync();
            if (text != null) sB.ProgressIndicator.Text = text;
            var appView = ApplicationView.GetForCurrentView();
            if (boundsMode != null && boundsMode.HasValue)
                appView.SetDesiredBoundsMode(boundsMode.Value);
            sB.ProgressIndicator.ProgressValue = progress;
        }

        public static void SetDefaultForPage(Type type)
        {
            if (type == typeof(MainPageHome) || type == typeof(ArtistShowsPage))
                StatusBarHelper.Default();
            else StatusBarHelper.SetTransparent();
        }
    }
#endif
}