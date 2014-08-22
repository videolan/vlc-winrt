using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Views.VideoPages;

namespace VLC_WINRT_APP.Helpers
{
    public static class VideoHUDHelper
    {
        public static void ShowLittleTextWithFadeOut(string msg)
        {
            App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (App.ApplicationFrame.CurrentSourcePageType != typeof (VideoPlayerPage))
                    return;
                ((App.ApplicationFrame.Content as VideoPlayerPage).Hud.Content as TextBlock).Text = msg;
                (App.ApplicationFrame.Content as
                    VideoPlayerPage).Hud.Show();
            });
        }
    }
}
