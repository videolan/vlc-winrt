/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace VLC_WINRT.Utility.Helpers.AwaitableUI
{
    public static class WriteableBitmapExtensions
    {
        /// <summary>
        /// Waits for the given WriteableBitmap to be loaded (non-zero size).
        /// </summary>
        /// <param name="wb">The WriteableBitmap to wait for.</param>
        /// <param name="timeoutInMs">The timeout in ms after which the wait will be cancelled. Use 0 to wait without a timeout.</param>
        /// <returns></returns>
        public static async Task WaitForLoaded(this WriteableBitmap wb, int timeoutInMs = 0)
        {
            int totalWait = 0;

            while (
                wb.PixelWidth <= 1 &&
                wb.PixelHeight <= 1)
            {
                await Task.Delay(10);
                totalWait += 10;

                if (timeoutInMs > 0 &&
                    totalWait > timeoutInMs)
                    return;
            }
        }
    }
}
