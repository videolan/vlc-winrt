using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace VLC_WINRT_APP.Helpers
{
    public static class ExceptionHelper
    {
        public static async Task ExceptionLogCheckup() 
        {
            if (ApplicationSettingsHelper.Contains("ExceptionLog"))
            {
                var resourcesLoader = ResourceLoader.GetForCurrentView("Resources");
                var dialog =
                    new MessageDialog(resourcesLoader.GetString("CrashReport"), resourcesLoader.GetString("WeNeedYourHelp"));
                dialog.Commands.Add(new UICommand(resourcesLoader.GetString("Yes"), async command =>
                {
                    var uri =
                        new Uri("mailto:modernvlc@outlook.com?subject=VLC for Windows 8.1 bugreport&body=" +
                                ApplicationSettingsHelper.ReadResetSettingsValue("ExceptionLog").ToString());
                    await Launcher.LaunchUriAsync(uri);
                }));
                dialog.Commands.Add(new UICommand(resourcesLoader.GetString("No"), command =>
                {
                    ApplicationSettingsHelper.ReadResetSettingsValue("ExceptionLog");
                }));
                await dialog.ShowAsync();
            }
        }

        public static void ExceptionStringBuilder(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            Package thisPackage = Package.Current;
            PackageVersion version = thisPackage.Id.Version;
            string appVersion = string.Format("{0}.{1}.{2}.{3}",
                version.Major, version.Minor, version.Build, version.Revision);

            StringBuilder stringExceptionBuilder = new StringBuilder("Exception Log VLC for Modern Windows");
            stringExceptionBuilder.AppendLine(appVersion);
            stringExceptionBuilder.AppendLine("Date");
            stringExceptionBuilder.AppendLine(DateTime.Now.ToString());
            stringExceptionBuilder.AppendLine(" ");
            stringExceptionBuilder.AppendLine(DateTime.Now.TimeOfDay.ToString());
            stringExceptionBuilder.AppendLine();
            stringExceptionBuilder.AppendLine("Current Page:");
            stringExceptionBuilder.AppendLine(App.ApplicationFrame.CurrentSourcePageType.FullName);
            stringExceptionBuilder.AppendLine();
            stringExceptionBuilder.AppendLine("Exception:");
            stringExceptionBuilder.AppendLine(unhandledExceptionEventArgs.Message.ToString());
            stringExceptionBuilder.AppendLine(unhandledExceptionEventArgs.Exception.HelpLink);
            stringExceptionBuilder.AppendLine(unhandledExceptionEventArgs.Exception.Message);
            stringExceptionBuilder.AppendLine(unhandledExceptionEventArgs.Exception.Source);
            stringExceptionBuilder.AppendLine(unhandledExceptionEventArgs.Exception.StackTrace);
            if (unhandledExceptionEventArgs.Exception.Data != null)
            {
                foreach (DictionaryEntry entry in unhandledExceptionEventArgs.Exception.Data)
                {
                    stringExceptionBuilder.AppendLine(entry.Key + ";" + entry.Value);
                }
            }
            stringExceptionBuilder.AppendLine(unhandledExceptionEventArgs.Exception.HResult.ToString());
            if (unhandledExceptionEventArgs.Exception.InnerException != null)
                stringExceptionBuilder.AppendLine(unhandledExceptionEventArgs.Exception.InnerException.ToString());
            stringExceptionBuilder.AppendLine("IsHandled: " + unhandledExceptionEventArgs.Handled.ToString());
            stringExceptionBuilder.Replace("\r\n", "<br/>");
            ApplicationSettingsHelper.SaveSettingsValue("ExceptionLog", stringExceptionBuilder.ToString());
        }
    }
}
