using System.Linq;
using System.Reflection;
using Windows.UI;

namespace VLC_WinRT.Helpers
{

    public static class AppViewHelper
    {
        private static bool isFullscreen = false;
        private static dynamic titleBar;

        private static bool DoesPropertyExist(string prop, dynamic list)
        {
            foreach (dynamic property in list)
            {
                if (property.Name == prop)
                    return true;
            }
            return false;
        }

        public static void SetAppView()
        {
#if WINDOWS_APP
            try
            {
                var v = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
                var allProperties = v.GetType().GetRuntimeProperties();
                titleBar = allProperties.FirstOrDefault(x => x.Name == "TitleBar");
                if (titleBar == null) return;
                dynamic bb = titleBar.GetMethod.Invoke(v, null);
                if (bb != null)
                {
                    var appViewProperties = bb.GetType().DeclaredProperties;
                    bb.BackgroundColor = (Color)App.Current.Resources["StatusBarColorBase"];
                    bb.ForegroundColor = Colors.WhiteSmoke;

                    bb.ButtonForegroundColor = Colors.WhiteSmoke;
                    if (DoesPropertyExist("InactiveBackgroundColor", appViewProperties))
                        bb.InactiveBackgroundColor = (Color)App.Current.Resources["InactiveMainColorBase"];
                    if (DoesPropertyExist("ButtonInactiveForegroundColor", appViewProperties))
                        bb.ButtonInactiveForegroundColor = Colors.WhiteSmoke;
                    if (DoesPropertyExist("InactiveForegroundColor", appViewProperties))
                        bb.InactiveForegroundColor = Colors.WhiteSmoke;

                    bb.ButtonBackgroundColor = (Color)App.Current.Resources["StatusBarColorBase"];
                    if (DoesPropertyExist("ButtonInactiveBackgroundColor", appViewProperties))
                        bb.ButtonInactiveBackgroundColor = (Color)App.Current.Resources["InactiveMainColorBase"];
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
            var v = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
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
    }
}
