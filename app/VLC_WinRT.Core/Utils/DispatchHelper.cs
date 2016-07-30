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

namespace VLC.Utils
{
    public class DispatchHelper
    {
        public static Task InvokeAsync(CoreDispatcherPriority priority, Action action)
        {
            return Task.Run(() => CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action()).AsTask());
        }
    }
}
