using NotificationsExtensions.Toasts;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VLC.Utils;
using VLC.ViewModels;
using Windows.ApplicationModel.Activation;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace VLC.Helpers
{
    public static class ToastHelper
    {
        public static void HandleProtocolActivation(IActivatedEventArgs args)
        {
            var toastArgs = args as ToastNotificationActivatedEventArgs;
            switch (toastArgs.Argument)
            {
                case "musicplayerview":
                    Locator.NavigationService.Go(Model.VLCPage.MusicPlayerPage);
                    break;
                case "playlistview":
                    Locator.NavigationService.Go(Model.VLCPage.MainPageMusic);
                    Locator.MusicLibraryVM.MusicView = Model.Music.MusicView.Playlists;
                    break;
            }
        }

        public static void Basic(string msg, bool playJingle = false, string toastId = "", string uri = "")
        {
            var toastContent = new ToastContent();
            if (!string.IsNullOrEmpty(uri))
                toastContent.Launch = uri;

            toastContent.Visual = new ToastVisual()
            {
                TitleText = new ToastText() { Text = "VLC" },
                BodyTextLine1 = new ToastText() { Text = msg }
            };

            toastContent.Audio = new ToastAudio() { Silent = !playJingle };

            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(toastContent.GetXml())
            {
                ExpirationTime = DateTimeOffset.Now.AddSeconds(15),
            });
        }

        public static void ToastImageAndText04(string title, string t1, string t2, string t3, string imgsrc, string toastId, string imgalt = "", string uri = "")
        {
            var toastContent = new ToastContent();
            if (!string.IsNullOrEmpty(uri))
                toastContent.Launch = uri;
            toastContent.Visual = new ToastVisual()
            {
                TitleText = new ToastText() { Text = title },
                BodyTextLine1 = new ToastText() { Text = t1 },
                BodyTextLine2 = new ToastText() { Text = t3 },
            };
            if (imgsrc != null)
            {
                toastContent.Visual.InlineImages.Add(new ToastImage()
                {
                    Source = new ToastImageSource(imgsrc),
                });
            }
            toastContent.Audio = new ToastAudio()
            {
                Silent = true,
            };
            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(toastContent.GetXml()));
        }
    }
}
