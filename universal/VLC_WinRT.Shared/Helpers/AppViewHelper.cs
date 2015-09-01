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

namespace VLC_WinRT.Helpers
{

    public static class AppViewHelper
    {
        private const double DefaultTitleBarHeight = 40;
        public static double TitleBarHeight
        {
            get
            {
                return DefaultTitleBarHeight;
                var height = AppViewHelper.SetTitleBarHeight();
                return height;
            }
        }

        public static double PreviousWindowHeight;
        public static double PreviousWindowsWidth;

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
            var isFullScreenModeProperty = v.GetType().GetRuntimeProperty("IsFullScreenMode"); // Available only in Windows 10
            if (isFullScreenModeProperty != null)
            {
                object isFullScreenMode = isFullScreenModeProperty.GetValue(v);
                return (bool)isFullScreenMode;
            }
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
                    bb.ButtonBackgroundColor = Color.FromArgb(0, 0, 0, 0);

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
            catch
            {
            }
#elif WINDOWS_UAP
            var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
#endif
        }

        public static void SetFullscreen(bool forceExit = false)
        {
#if WINDOWS_APP
            var v = ApplicationView.GetForCurrentView();
            var runtimeMethods = v.GetType().GetRuntimeMethods();

            if (IsFullScreen() || forceExit)
            {
                var exitFullScreenMode = runtimeMethods.FirstOrDefault(x => x.Name == "ExitFullScreenMode");
                exitFullScreenMode?.Invoke(v, null);
            }
            else
            {
                var tryEnterFullScreenMode = runtimeMethods.FirstOrDefault(x => x.Name == "TryEnterFullScreenMode");
                tryEnterFullScreenMode?.Invoke(v, null);
            }
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

        public static async void ResizeWindow(bool restoPreviousSize, double width = 0, double height = 0)
        {
#if WINDOWS_APP
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

        public static void SetTitleBar(bool extend)
        {
            var titleBarInstance = GetTitleBarInstanceOnW10();
            if (titleBarInstance == null) return;
            titleBarInstance.ExtendViewIntoTitleBar = extend;
        }

        public static double SetTitleBarHeight()
        {
#if WINDOWS_PHONE_APP
            return 0;
#else
            var titleBarInstance = GetTitleBarInstanceOnW10();
            if (titleBarInstance == null) return DefaultTitleBarHeight;
            if (titleBarInstance.Height == 0) return DefaultTitleBarHeight;
            return titleBarInstance.Height;
#endif
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