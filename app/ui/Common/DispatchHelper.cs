/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Threading.Tasks;
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
                CoreApplication.MainView.CoreWindow.
                    Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() => action()).AsTask().Wait();
            }
        }

        public static async Task InvokeAsync(Func<Task> action)
        { 
            //for some reason this crashes the designer (so dont do it in design mode)
            if (DesignMode.DesignModeEnabled) return;

            if (CoreApplication.MainView.CoreWindow == null || CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
            {
                await action();
            }
            else
            {
                await CoreApplication.MainView.CoreWindow.
                    Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
            }
        }
    }
}
