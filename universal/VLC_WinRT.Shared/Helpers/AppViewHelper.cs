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

namespace VLC_WinRT.Helpers
{

    public static class AppViewHelper
    {
        private static bool isFullscreen = false;
        private static dynamic titleBar;
        public static double TitleBarHeight = 32;
        static AppViewHelper()
        {
            DisplayInformation.GetForCurrentView().DpiChanged += AppViewHelper_DpiChanged;
        }

        private static async void AppViewHelper_DpiChanged(DisplayInformation sender, object args)
        {
            SetTitleBar(false);
            await Task.Delay(200);
            SetTitleBar(true);
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
                titleBar = allProperties.FirstOrDefault(x => x.Name == "TitleBar");
                if (titleBar == null) return;
                dynamic bb = titleBar.GetMethod.Invoke(v, null);
                if (bb != null)
                {
                    var appViewProperties = bb.GetType().DeclaredProperties;
                    bb.BackgroundColor = (Color)App.Current.Resources["ApplicationBarForegroundThemeColor"];
                    bb.ForegroundColor = fgColor;
                    bb.ButtonForegroundColor = fgColor;
                    bb.ButtonBackgroundColor = (Color)App.Current.Resources["ApplicationBarForegroundThemeColor"];
                    
                    if (DoesPropertyExist("InactiveBackgroundColor", appViewProperties))
                        bb.InactiveBackgroundColor = bb.BackgroundColor;
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
            var coreAppView = CoreApplication.GetCurrentView();
            var allProperties = coreAppView.GetType().GetRuntimeProperties();
            var titleBar = allProperties.FirstOrDefault(x => x.Name == "TitleBar");
            if (titleBar == null)
            {
                App.SplitShell.TitleBarHeight = TitleBarHeight;
                return;
            }
            dynamic titleBarInstance = titleBar.GetMethod.Invoke(coreAppView, null);
            var titleBarInstanceProperties = titleBarInstance.GetType().DeclaredProperties;
            if (titleBarInstanceProperties != null)
            {
                if (extend)
                {
                    var heightOriginal = Window.Current.Bounds.Height;
                    WindowSizeChangedEventHandler resizeHandler = null;
                    resizeHandler = (s, e) =>
                     {
                         Window.Current.SizeChanged -= resizeHandler;
                         var heightNew = Window.Current.Bounds.Height;
                         TitleBarHeight = heightNew - heightOriginal;
                         if (TitleBarHeight < 0)
                             TitleBarHeight = 32;
                         App.SplitShell.TitleBarHeight = TitleBarHeight;
                     };
                    Window.Current.SizeChanged += resizeHandler;
                }
                titleBarInstance.ExtendViewIntoTitleBar = extend;
            }
        }
    }
}
