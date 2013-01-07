using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace VLC_WINRT.Common
{
    public class DispatchHelper
    {
        public static void Invoke(Action action)
        {
            if (CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
            {
                action();
            }
            else
            {
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
            }
        }
    }
}