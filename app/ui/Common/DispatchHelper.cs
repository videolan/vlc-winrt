using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace VLC_WINRT.Common
{
    public class DispatchHelper
    {
        public static void Invoke(Action action)
        {
            //for some reason this crashes the designer (so dont do it in design mode)
            if (DesignMode.DesignModeEnabled) return;

            if (CoreApplication.MainView.CoreWindow == null || CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
            {
                action();
            }
            else
            {
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                                                                        () => action());
            }
        }
    }
}