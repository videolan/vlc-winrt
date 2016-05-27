using NotificationsExtensions.Toasts;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;
using Windows.ApplicationModel.Activation;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace VLC_WinRT.Helpers
{
    public static class ToastHelper
    {
        public static async Task HandleProtocolActivation(IActivatedEventArgs args)
        {
            var toastArgs = args as ToastNotificationActivatedEventArgs;
            switch (toastArgs.Argument)
            {
                case "musicplayerview":
                    Locator.NavigationService.Go(Model.VLCPage.MusicPlayerPage);
                    break;
            }
        }

        public static void Basic(string msg, bool playJingle = false, string toastId = "")
        {
            ToastTemplateType toastTemplate = ToastTemplateType.ToastText01;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(msg));
            if (!playJingle)
            {
                IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
                XmlElement audio = toastXml.CreateElement("audio");
                audio.SetAttribute("silent", "true");
                toastNode?.AppendChild(audio);
            }

            ToastNotification toast = new ToastNotification(toastXml);
            var nameProperty = toast.GetType().GetRuntimeProperties().FirstOrDefault(x => x.Name == "Tag");
            if (nameProperty != null && !string.IsNullOrEmpty(toastId))
            {
                nameProperty.SetValue(toast, toastId);
            }
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        public static void ToastImageAndText04(string t1, string t2, string t3, string imgsrc, string toastId, string imgalt = "")
        {
#if WINDOWS_UWP
            var toastContent = new ToastContent();
            toastContent.Launch = "musicplayerview";
            toastContent.Visual = new ToastVisual()
            {
                TitleText = new ToastText() { Text = $"VLC {Strings.Dash} {Strings.NowPlaying}" },
                BodyTextLine1 = new ToastText() { Text = t1 },
                BodyTextLine2 = new ToastText() { Text = t3 },
            };
            toastContent.Visual.InlineImages.Add(new ToastImage()
            {
                Source = new ToastImageSource(imgsrc),
            });
            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(toastContent.GetXml()));
#else
            ToastTemplateType toastTemplate = ToastTemplateType.ToastImageAndText04;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(t1));
            if(t2 != null)
                toastTextElements[1].AppendChild(toastXml.CreateTextNode(t2));
            if(t3 != null)
                toastTextElements[2].AppendChild(toastXml.CreateTextNode(t3));

            if (!string.IsNullOrEmpty(imgsrc))
            {
                XmlNodeList toastImgElement = toastXml.GetElementsByTagName("image");
                ((XmlElement)toastImgElement[0]).SetAttribute("src", imgsrc);
                ((XmlElement)toastImgElement[0]).SetAttribute("alt", imgalt);
            }
            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            XmlElement audio = toastXml.CreateElement("audio");
            audio.SetAttribute("silent", "true");
            toastNode?.AppendChild(audio);

            ToastNotification toast = new ToastNotification(toastXml);
            var nameProperty = toast.GetType().GetRuntimeProperties().FirstOrDefault(x => x.Name == "Tag");
            if (nameProperty != null && !string.IsNullOrEmpty(toastId))
            {
                nameProperty.SetValue(toast, toastId);
            }
            ToastNotificationManager.CreateToastNotifier().Show(toast);
#endif
        }
    }
}
