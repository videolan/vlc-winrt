using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;

namespace VLC_WinRT.Helpers
{
    public static class OSHelper
    {
        public static bool IsWindows10
        {
            get
            {
                var isFullScreenModeProperty = typeof(ApplicationView).GetRuntimeProperty("IsFullScreenMode");
                return isFullScreenModeProperty != null;
            }
        }
    }
}
