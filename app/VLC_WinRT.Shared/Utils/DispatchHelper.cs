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
        static readonly SemaphoreSlim ThreadUISemaphoreSlim = new SemaphoreSlim(0);

        public static async Task InvokeAsync(CoreDispatcherPriority priority, Action action)
        {
            ThreadUISemaphoreSlim.Wait();

            try
            {
                //for some reason this crashes the designer (so dont do it in design mode)
                if (!DesignMode.DesignModeEnabled)
                {
                    if (CoreApplication.MainView.CoreWindow == null || CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
                    {
                        action();
                    }
                    else
                    {
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
                    }
                }
            }
            catch { ThreadUISemaphoreSlim.Release(); }
            finally
            {
                ThreadUISemaphoreSlim.Release();
            }
        }

        // This should be avoided as much as possible as it will block a thread.
        // Prefer the asynchronous version whenever possible
        public static void Invoke(Action action)
        {
            InvokeAsync(CoreDispatcherPriority.Normal, action).Wait();
        }
    }
}
