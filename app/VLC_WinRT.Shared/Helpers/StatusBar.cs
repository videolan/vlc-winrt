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
        public static async Task Initialize()
        {
            var appView = ApplicationView.GetForCurrentView();
            var sB = StatusBar.GetForCurrentView();
            await sB.HideAsync();
            Responsive(appView);
            appView.VisibleBoundsChanged += StatusBarHelper_VisibleBoundsChanged;
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
                var rect = OccludedRect;
                App.SplitShell.Margin = new Thickness(0, statusBarHeight, 0, navBarHeight);
            }
            else
            {
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