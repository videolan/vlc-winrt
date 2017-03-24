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
        public async static Task InvokeInUIThreadAsync(CoreDispatcherPriority priority, Action action)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(priority, () => {action();});
        }

        public async static Task<bool> InvokeInUIThread(CoreDispatcherPriority priority, Action action)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            await CoreApplication.MainView.Dispatcher.RunAsync(priority, () => {
                try
                {
                    action();
                    taskCompletionSource.SetResult(true);
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            });
            return await taskCompletionSource.Task;
        }

        public async static Task<T> InvokeInUIThread<T>(CoreDispatcherPriority priority, Func<T> action)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();
            await CoreApplication.MainView.Dispatcher.RunAsync(priority, () => {
                try
                {
                    T ret = action();
                    taskCompletionSource.SetResult(ret);
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            });
            return await taskCompletionSource.Task;
        }

        public async static Task InvokeInUIThreadHighPriority(Action action)
        {
            await InvokeInUIThread(CoreDispatcherPriority.High, action);
        }
    }
}
