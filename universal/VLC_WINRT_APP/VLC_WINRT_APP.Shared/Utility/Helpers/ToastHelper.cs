using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace VLC_WINRT_APP.Utility.Helpers
{
    public static class ToastHelper
    {
        public static void Basic(string msg, bool playJingle = false)
        {
            ToastTemplateType toastTemplate = ToastTemplateType.ToastText01;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(msg));
            if (!playJingle)
            {
                var audio = toastXml.CreateElement("audio");
                audio.SetAttribute("silent", "true");
            }
            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}
