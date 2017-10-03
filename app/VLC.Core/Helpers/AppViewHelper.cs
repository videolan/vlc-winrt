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
using VLC.ViewModels;
using Windows.Foundation.Metadata;
using VLC.Utils;

namespace VLC.Helpers
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

        public static double TitleBarRightOffset
        {
            get
            {
#if WINDOWS_UWP
                return CoreApplication.GetCurrentView().TitleBar.SystemOverlayRightInset;
#else
                return 0;
#endif
            }
        }

        static AppViewHelper()
        {
        }

        public static async Task SetAppView(bool extend)
        {
#if WINDOWS_UWP
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                await StatusBar.GetForCurrentView().HideAsync();
            }

            if (DeviceHelper.GetDeviceType() == DeviceTypeEnum.Xbox)
            {
                ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            }

            if (DeviceHelper.GetDeviceType() != DeviceTypeEnum.Tablet)
                return;
            if (Numbers.OSVersion <= 10586)
                return;

            var coreAppViewTitleBar = CoreApplication.GetCurrentView().TitleBar;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;

            
            if (!Locator.SettingsVM.MediaCenterMode)
            {
                coreAppViewTitleBar.ExtendViewIntoTitleBar = extend;
                if (extend)
                {
                    titleBar.BackgroundColor = Colors.Transparent;
                    titleBar.ButtonBackgroundColor = Colors.Transparent;

                    titleBar.ButtonForegroundColor = Colors.White;
                }
                else
                {
                    titleBar.BackgroundColor = Colors.DimGray;
                    titleBar.InactiveBackgroundColor = Colors.DarkGray;
                    
                    titleBar.ButtonBackgroundColor = Colors.DimGray;
                    titleBar.ButtonInactiveBackgroundColor = Colors.DarkGray;

                    titleBar.ButtonForegroundColor = Colors.White;
                }
            }
            else
                coreAppViewTitleBar.ExtendViewIntoTitleBar = false;
#endif


        }

        public static void SetTitleBar(UIElement titleBar)
        {
            if (DeviceHelper.GetDeviceType() != DeviceTypeEnum.Tablet)
                return;
            if (Numbers.OSVersion <= 10586)
                return;
            //Window.Current.SetTitleBar(titleBar);
        }

        public static void SetTitleBarTitle(string title = null)
        {
            var appView = ApplicationView.GetForCurrentView();
            if (string.IsNullOrEmpty(title))
                title = string.Empty;
            appView.Title = title;
        }

        public static void ToggleFullscreen()
        {
            var v = ApplicationView.GetForCurrentView();

            //if (v.IsFullScreenMode)
            //{
            //    v.ExitFullScreenMode();
            //}
            //else
            //{
            //    v.TryEnterFullScreenMode();
            //}
        }

        public static void LeaveFullscreen()
        {
            var v = ApplicationView.GetForCurrentView();
            //v.ExitFullScreenMode();
        }

        public static void EnterFullscreen()
        {
            var v = ApplicationView.GetForCurrentView();
            //v.TryEnterFullScreenMode();
        }

        public static bool GetFullscreen()
        {
            //return ApplicationView.GetForCurrentView().IsFullScreenMode;
            return false;
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
        }
    }
}