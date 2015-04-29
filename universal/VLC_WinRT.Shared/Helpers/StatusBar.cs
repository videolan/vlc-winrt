#if WINDOWS_PHONE_APP
using System;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using System.Diagnostics;
using Windows.Graphics.Display;
using Windows.UI;

namespace VLC_WinRT.Helpers
{
    public static class StatusBarHelper
    {
        static StatusBarHelper()
        {
            //DisplayInformation.GetForCurrentView().OrientationChanged += MainPage_OrientationChanged;
            Responsive(ApplicationView.GetForCurrentView());
        }

        public static void Default()
        {
            Set(App.Current.Resources["StatusBarColor"] as SolidColorBrush, null, 1, "", ApplicationViewBoundsMode.UseVisible);
        }

        static void Set(SolidColorBrush background, SolidColorBrush foreground, double opacity, string text, ApplicationViewBoundsMode? boundsMode = ApplicationViewBoundsMode.UseVisible, double? progress = 0)
        {
            ApplicationView.GetForCurrentView().VisibleBoundsChanged += StatusBarHelper_VisibleBoundsChanged;
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

        private static void StatusBarHelper_VisibleBoundsChanged(ApplicationView sender, object args)
        {
            Responsive(sender);
        }

        static async void Responsive(ApplicationView sender)
        {
            if (DisplayHelper.IsPortrait())
            {
                var screenHeight = Window.Current.Bounds.Height;
                var statusBarHeight = OccludedRect.Height;
                var tmpAppViewHeight = screenHeight - statusBarHeight;
                var navBarHeight = tmpAppViewHeight - sender.VisibleBounds.Height;
                await Show();
                var rect = OccludedRect;
                App.SplitShell.Margin = new Thickness(0, statusBarHeight, 0, navBarHeight);
            }
            else
            {
                await Hide();
                var screenWidth = Window.Current.Bounds.Width;
                var tmpAppViewWidth = sender.VisibleBounds.Width;
                double navBarWidth;
                navBarWidth = screenWidth - tmpAppViewWidth;
                App.SplitShell.Margin = new Thickness((sender.VisibleBounds.Left > 0) ? navBarWidth : 0,
                                                               0,
                                                               (sender.VisibleBounds.Left > 0) ? 0 : navBarWidth,
                                                               0);
            }
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