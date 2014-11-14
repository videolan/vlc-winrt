using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
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
                var dialog =
                    new MessageDialog(
                        "VLC has crashed and made an automatic bug report. Do you allow us to sent it?",
                        "We need your help");
                dialog.Commands.Add(new UICommand("Oui", async command =>
                {
                    var uri =
                        new Uri("mailto:vlcmetro-feedback@outlook.com?subject=VLC for Windows 8.1 bugreport&body=" +
                                ApplicationSettingsHelper.ReadResetSettingsValue("ExceptionLog").ToString());
                    await Launcher.LaunchUriAsync(uri);
                }));
                dialog.Commands.Add(new UICommand("Non", command =>
                {
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
            stringExceptionBuilder.AppendLine(unhandledExceptionEventArgs.Exception.Data.ToString());
            stringExceptionBuilder.AppendLine(unhandledExceptionEventArgs.Exception.HResult.ToString());
            stringExceptionBuilder.AppendLine(unhandledExceptionEventArgs.Exception.InnerException.ToString());
            stringExceptionBuilder.AppendLine(unhandledExceptionEventArgs.Handled.ToString());
            ApplicationSettingsHelper.SaveSettingsValue("ExceptionLog", stringExceptionBuilder.ToString());
        }
    }
}
