using System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Views.VideoPages;

namespace VLC_WINRT_APP.Helpers
{
    public static class VideoHUDHelper
    {
        public static async void ShowLittleTextWithFadeOut(string msg)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (App.ApplicationFrame.CurrentSourcePageType != typeof (VideoPlayerPage))
                    return;
                var page = App.ApplicationFrame.Content as VideoPlayerPage;
                (page.Hud.Content as TextBlock).Text = msg;
                await page.Hud.Show();
            });
        }
    }
}
