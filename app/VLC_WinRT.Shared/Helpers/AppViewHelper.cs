using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.ViewModels;
using Windows.Foundation.Metadata;
using VLC_WinRT.UI.Legacy.Views.UserControls;
using VLC_WinRT.Views.UserControls;

namespace VLC_WinRT.Helpers
{

    public static class AppViewHelper
    {
        public static double PreviousWindowHeight;
        public static double PreviousWindowsWidth;
        public static double TitleBarHeight
        {
            get
            {
#if WINDOWS_UWP
                return CoreApplication.GetCurrentView().TitleBar.Height;
#else
                return 32;
#endif
            }
        }

        static AppViewHelper()
        {
        }

        public static void SetAppView(bool extend)
        {
#if WINDOWS_UWP
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = extend;
            var appView = ApplicationView.GetForCurrentView();
            var titleBar = appView.TitleBar;
            titleBar.ButtonForegroundColor = Colors.DimGray;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                Windows.UI.ViewManagement.StatusBar.GetForCurrentView().HideAsync();
            }
#endif
        }

        public static void SetFullscreen()
        {
#if WINDOWS_UWP
            var v = ApplicationView.GetForCurrentView();

            if (v.IsFullScreenMode)
            {
                v.ExitFullScreenMode();
            }
            else
            {
                v.TryEnterFullScreenMode();
            }
#endif
        }

        public static bool GetFullscreen()
        {
            var v = ApplicationView.GetForCurrentView();
#if WINDOWS_UWP
            return v.IsFullScreenMode;
#else
            return true;
#endif
        }

        public static async Task CreateNewWindow(Type view, double width, double height)
        {
#if WINDOWS_UWP
            var newCoreAppView = CoreApplication.CreateNewView();
            var appView = ApplicationView.GetForCurrentView();
            await newCoreAppView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, async () =>
            {
                var window = Window.Current;
                var newAppView = ApplicationView.GetForCurrentView();

                var allMethods = newAppView.GetType().GetRuntimeMethods();
                var setPrefferedMinSize = allMethods.FirstOrDefault(x => x.Name == "SetPreferredMinSize");
                if (setPrefferedMinSize != null)
                {
                    setPrefferedMinSize.Invoke(newAppView, new object[1]
                    {
                        new Size(width, height),
                    });
                }

                var frame = new Frame();
                window.Content = frame;
                frame.Navigate(view);
                window.Activate();

                await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newAppView.Id, ViewSizePreference.UseMore, appView.Id, ViewSizePreference.Default);
                var tryResizeView = allMethods.FirstOrDefault(x => x.Name == "TryResizeView");
                if (tryResizeView != null)
                {
                    tryResizeView.Invoke(newAppView, new object[1]
                    {
                        new Size(width, height),
                    });
                }
            });
#endif
        }

        public static async void ResizeWindow(bool restoPreviousSize, double width = 0, double height = 0)
        {
#if WINDOWS_UWP
            var appView = ApplicationView.GetForCurrentView();
            var allMethods = appView.GetType().GetRuntimeMethods();
            var setPrefferedMinSize = allMethods.FirstOrDefault(x => x.Name == "SetPreferredMinSize");
            if (setPrefferedMinSize != null)
            {
                if (restoPreviousSize)
                {
                    setPrefferedMinSize.Invoke(appView, new object[1]
                    {
                        new Size(PreviousWindowsWidth, PreviousWindowHeight),
                    });
                }
                else
                {
                    setPrefferedMinSize.Invoke(appView, new object[1]
                    {
                        new Size(width, height),
                    });
                }
            }
            await Task.Delay(100);
            var tryResizeView = allMethods.FirstOrDefault(x => x.Name == "TryResizeView");
            if (tryResizeView != null)
            {
                if (restoPreviousSize)
                {
                    tryResizeView.Invoke(appView, new object[1]
                    {
                        new Size(PreviousWindowsWidth, PreviousWindowHeight),
                    });
                }
                else
                {
                    tryResizeView.Invoke(appView, new object[1]
                    {
                        new Size(width, height),
                    });
                    PreviousWindowHeight = Window.Current.Bounds.Height;
                    PreviousWindowsWidth = Window.Current.Bounds.Width;
                }
            }
#endif
        }
    }
}