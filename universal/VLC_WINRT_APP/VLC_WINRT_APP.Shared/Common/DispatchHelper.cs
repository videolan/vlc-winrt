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
        public static Task InvokeAsync(Action action)
        {
            //for some reason this crashes the designer (so dont do it in design mode)
            if (DesignMode.DesignModeEnabled) return Task.FromResult<bool>(false);

            if (CoreApplication.MainView.CoreWindow == null || CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
            {
                action();
                return Task.FromResult<bool>(true);
            }
            else
            {
                return CoreApplication.MainView.CoreWindow.
                    Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action()).AsTask();
            }
        }

        // This should be avoided as much as possible as it will block a thread.
        // Prefer the asynchronous version whenever possible
        public static void Invoke(Action action)
        {
            InvokeAsync(action).Wait();
        }
    }
}
