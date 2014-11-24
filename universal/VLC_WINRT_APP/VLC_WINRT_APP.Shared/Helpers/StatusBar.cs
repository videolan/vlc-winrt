using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;

namespace VLC_WINRT_APP.Helpers
{
#if WINDOWS_PHONE_APP
    public static class StatusBarHelper
    {
        public static void Default()
        {
            Set(App.Current.Resources["MainColor"] as SolidColorBrush, 0, "VLC", ApplicationViewBoundsMode.UseCoreWindow);
        }

        public static void SetTransparent()
        {
            Set(null, 0, "VLC", ApplicationViewBoundsMode.UseCoreWindow);
        }

        static void Set(SolidColorBrush background, double opacity, string text, ApplicationViewBoundsMode boundsMode)
        {
            StatusBar sB = StatusBar.GetForCurrentView();
            if (background != null) 
                sB.BackgroundColor = background.Color;
            sB.BackgroundOpacity = opacity;
            var _ = sB.ProgressIndicator.ShowAsync();
            if (text != null) sB.ProgressIndicator.Text = text;
            var appView = ApplicationView.GetForCurrentView();
            appView.SetDesiredBoundsMode(boundsMode);

        }
    }
#endif
}