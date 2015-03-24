using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Windows.UI;

namespace VLC_WINRT_APP.Helpers
{

    public static class AppViewHelper
    {
        private static dynamic titleBar;

        public static void SetAppView()
        {
#if WINDOWS_APP
            var v = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            var allProperties = v.GetType().GetRuntimeProperties();
            titleBar = allProperties.FirstOrDefault(x => x.Name == "TitleBar");
            if (titleBar == null) return;
            dynamic bb = titleBar.GetMethod.Invoke(v, null);
            bb.BackgroundColor = (Color)App.Current.Resources["MainColorBase"];
            bb.ForegroundColor = Colors.WhiteSmoke;
            bb.ButtonForegroundColor = Colors.WhiteSmoke;
#endif
        }

        public static void SetBackgroundButtonColor()
        {
#if WINDOWS_APP
            var v = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            var allProperties = v.GetType().GetRuntimeProperties();
            titleBar = allProperties.FirstOrDefault(x => x.Name == "TitleBar");
            if (titleBar == null) return;
            dynamic bb = titleBar.GetMethod.Invoke(v, null);
            bb.ButtonBackgroundColor = (Color) App.Current.Resources["MainColorBase"];
#endif
        }

        public static void SetFullscren(bool isfullscreen)
        {
#if WINDOWS_APP
            var v = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            var runtimeMethods = v.GetType().GetRuntimeMethods();
            if (isfullscreen)
            {
                var tryEnterFullScreenMode = runtimeMethods.FirstOrDefault(x => x.Name == "TryEnterFullScreenMode");
                if (tryEnterFullScreenMode == null) return;
                tryEnterFullScreenMode.Invoke(v, null);
            }
            else
            {
                var exitFullScreenMode = runtimeMethods.FirstOrDefault(x => x.Name == "ExitFullScreenMode");
                if (exitFullScreenMode == null) return;
                exitFullScreenMode.Invoke(v, null);
            }
#endif
        }
    }
}
