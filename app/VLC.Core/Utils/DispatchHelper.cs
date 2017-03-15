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
        public async static Task InvokeAsync(CoreDispatcherPriority priority, Action action)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(priority, () => action());
        }

        public async static Task<T> InvokeAsync<T>(CoreDispatcherPriority priority, Func<T> action)
        {
            T result = default(T);
            await CoreApplication.MainView.Dispatcher.RunAsync(priority, () => result = action());
            return result;
        }

        public async static Task InvokeAsyncHighPriority(Action action)
        {
            await InvokeAsync(CoreDispatcherPriority.High, action);
        }
    }
}
