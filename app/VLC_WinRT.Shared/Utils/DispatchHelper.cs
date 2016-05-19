/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace VLC_WinRT.Utils
{
    public class DispatchHelper
    {
        public static Task InvokeAsync(CoreDispatcherPriority priority, Action action)
        {
            if (CoreApplication.MainView.Dispatcher.HasThreadAccess)
            {
                action();
                return Task.FromResult(true);
            }
            return CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action()).AsTask();
        }
    }
}
