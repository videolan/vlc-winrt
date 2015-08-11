using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace VLC_WinRT.Helpers
{

    public static class AppViewHelper
    {
        private static bool isFullscreen = false;
        public static double TitleBarHeight = 32;

        static AppViewHelper()
        {
            DisplayInformation.GetForCurrentView().DpiChanged += AppViewHelper_DpiChanged;
        }

        private static void AppViewHelper_DpiChanged(DisplayInformation sender, object args)
        {
            SetTitleBarHeight();
        }

        public static bool IsFullScreen()
        {
            var v = ApplicationView.GetForCurrentView();
            return v.IsFullScreen;
        }

        private static bool DoesPropertyExist(string prop, dynamic list)
        {
            foreach (dynamic property in list)
            {
                if (property.Name == prop)
                    return true;
            }
            return false;
        }

        public static void SetAppView(Color fgColor)
        {
#if WINDOWS_APP
            try
            {
                var v = ApplicationView.GetForCurrentView();
                var allProperties = v.GetType().GetRuntimeProperties();
                var titleBar = allProperties.FirstOrDefault(x => x.Name == "TitleBar");
                if (titleBar == null) return;
                dynamic bb = titleBar.GetMethod.Invoke(v, null);
                if (bb != null)
                {
                    var appViewProperties = bb.GetType().DeclaredProperties;
                    bb.BackgroundColor = Color.FromArgb(0, 0, 0, 0);
                    bb.ForegroundColor = fgColor;
                    bb.ButtonForegroundColor = fgColor;
                    bb.ButtonBackgroundColor = Color.FromArgb(0,0,0,0);

                    if (DoesPropertyExist("InactiveBackgroundColor", appViewProperties))
                        bb.InactiveBackgroundColor = Color.FromArgb(0, 0, 0, 0);
                    if (DoesPropertyExist("ButtonInactiveForegroundColor", appViewProperties))
                        bb.ButtonInactiveForegroundColor = bb.ButtonForegroundColor;
                    if (DoesPropertyExist("InactiveForegroundColor", appViewProperties))
                        bb.InactiveForegroundColor = bb.ButtonForegroundColor;
                    if (DoesPropertyExist("ButtonInactiveBackgroundColor", appViewProperties))
                        bb.ButtonInactiveBackgroundColor = bb.BackgroundColor;
                }
            }
            catch { }
#elif WINDOWS_UAP
            var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
#endif
        }
        
        public static async void SetFullscreen()
        {
#if WINDOWS_APP
            var v = ApplicationView.GetForCurrentView();
            var runtimeMethods = v.GetType().GetRuntimeMethods();
            
            if (!isFullscreen)
            {
                var tryEnterFullScreenMode = runtimeMethods.FirstOrDefault(x => x.Name == "TryEnterFullScreenMode");
                tryEnterFullScreenMode?.Invoke(v, null);
            }
            else
            {
                var exitFullScreenMode = runtimeMethods.FirstOrDefault(x => x.Name == "ExitFullScreenMode");
                exitFullScreenMode?.Invoke(v, null);
            }
            isFullscreen = !isFullscreen;
#endif
        }


        public static async Task CreateNewWindow(Type view, double width, double height)
        {
#if WINDOWS_APP
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

        public static void ResizeWindow(double width, double height)
        {
#if WINDOWS_APP
            var appView = ApplicationView.GetForCurrentView();
            var allMethods = appView.GetType().GetRuntimeMethods();
            var setPrefferedMinSize = allMethods.FirstOrDefault(x => x.Name == "SetPreferredMinSize");
            if (setPrefferedMinSize != null)
            {
                setPrefferedMinSize.Invoke(appView, new object[1]
                {
                        new Size(width, height),
                });
            }
            var tryResizeView = allMethods.FirstOrDefault(x => x.Name == "TryResizeView");
            if (tryResizeView != null)
            {
                tryResizeView.Invoke(appView, new object[1]
                {
                    new Size(width, height),
                });
            }
#endif
        }

        public static void SetTitleBar(bool extend)
        {
            var titleBarInstance = GetTitleBarInstanceOnW10();
            if (titleBarInstance == null) return;
            titleBarInstance.ExtendViewIntoTitleBar = extend;
        }

        public static void SetTitleBarHeight()
        {
            var titleBarInstance = GetTitleBarInstanceOnW10();
            if (titleBarInstance == null) return;
            if (titleBarInstance.Height == 0) return;
            TitleBarHeight = titleBarInstance.Height;
            App.SplitShell.TitleBarHeight = TitleBarHeight;
        }

        public static dynamic GetTitleBarInstanceOnW10()
        {
            var coreAppView = CoreApplication.GetCurrentView();
            var allProperties = coreAppView.GetType().GetRuntimeProperties();
            var titleBar = allProperties.FirstOrDefault(x => x.Name == "TitleBar");
            dynamic titleBarInstance = titleBar?.GetMethod.Invoke(coreAppView, null);
            return titleBarInstance;
        }
    }
}