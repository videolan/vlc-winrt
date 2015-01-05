using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.System;
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
                Package thisPackage = Package.Current;
                PackageVersion version = thisPackage.Id.Version;
                string appVersion = string.Format("{0}.{1}.{2}.{3}",
                    version.Major, version.Minor, version.Build, version.Revision);

                var resourcesLoader = ResourceLoader.GetForCurrentView("Resources");
                var dialog =
                    new MessageDialog(resourcesLoader.GetString("CrashReport"), resourcesLoader.GetString("WeNeedYourHelp"));
                dialog.Commands.Add(new UICommand(resourcesLoader.GetString("Yes"), async command =>
                {
                    string os = null;
#if WINDOWS_APP
                    os = "Windows 8.1 v" + appVersion;
#else
                    os = "Windows Phone 8.1 v" + appVersion;
#endif
                    var uri =
                        new Uri("mailto:modernvlc@outlook.com?subject=" + os + "&body=" +
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
            if (App.ApplicationFrame != null && App.ApplicationFrame.CurrentSourcePageType != null)
            {
                stringExceptionBuilder.AppendLine(App.ApplicationFrame.CurrentSourcePageType.FullName);
            }
            else
            {
                stringExceptionBuilder.AppendLine("Null");
            }
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

#if WINDOWS_PHONE_APP
            // Gets the app's current memory usage    
            ulong AppMemoryUsageUlong = MemoryManager.AppMemoryUsage;
            // Gets the app's memory usage limit    
            ulong AppMemoryUsageLimitUlong = MemoryManager.AppMemoryUsageLimit;

            AppMemoryUsageUlong /= 1024 * 1024;
            AppMemoryUsageLimitUlong /= 1024 * 1024;
            stringExceptionBuilder.AppendLine("CurrentRAM:" + AppMemoryUsageUlong + " -- ");
            stringExceptionBuilder.AppendLine("MaxRAM:" + AppMemoryUsageLimitUlong + " -- ");
            stringExceptionBuilder.AppendLine("CommentOnRAM:" + MemoryManager.AppMemoryUsageLevel.ToString());
#endif
            ApplicationSettingsHelper.SaveSettingsValue("ExceptionLog", stringExceptionBuilder.ToString());
        }

        public static void CreateMemorizedException(string method, Exception exception)
        {
            StringBuilder stringExceptionBuilder = new StringBuilder("Exception Log VLC for Modern Windows");
            stringExceptionBuilder.AppendLine("Exception at : " + method);
            stringExceptionBuilder.AppendLine("Date");
            stringExceptionBuilder.AppendLine(DateTime.Now.ToString());
            stringExceptionBuilder.AppendLine(" ");
            stringExceptionBuilder.AppendLine(DateTime.Now.TimeOfDay.ToString());
            stringExceptionBuilder.AppendLine();
            stringExceptionBuilder.AppendLine("Current Page:");
            if (App.ApplicationFrame != null && App.ApplicationFrame.CurrentSourcePageType != null)
            {
                stringExceptionBuilder.AppendLine(App.ApplicationFrame.CurrentSourcePageType.FullName);
            }
            else
            {
                stringExceptionBuilder.AppendLine("Page Null");
            }

            stringExceptionBuilder.AppendLine(exception.Message.ToString());
            stringExceptionBuilder.AppendLine(exception.HelpLink);
            stringExceptionBuilder.AppendLine(exception.Message);
            stringExceptionBuilder.AppendLine(exception.Source);
            stringExceptionBuilder.AppendLine(exception.StackTrace);
            if (exception.Data != null)
            {
                foreach (DictionaryEntry entry in exception.Data)
                {
                    stringExceptionBuilder.AppendLine(entry.Key + ";" + entry.Value);
                }
            }

#if WINDOWS_PHONE_APP
            // Gets the app's current memory usage    
            ulong AppMemoryUsageUlong = MemoryManager.AppMemoryUsage;
            // Gets the app's memory usage limit    
            ulong AppMemoryUsageLimitUlong = MemoryManager.AppMemoryUsageLimit;

            AppMemoryUsageUlong /= 1024 * 1024;
            AppMemoryUsageLimitUlong /= 1024 * 1024;
            stringExceptionBuilder.AppendLine("CurrentRAM:" + AppMemoryUsageUlong + " -- ");
            stringExceptionBuilder.AppendLine("MaxRAM:" + AppMemoryUsageLimitUlong + " -- ");
            stringExceptionBuilder.AppendLine("CommentOnRAM:" + MemoryManager.AppMemoryUsageLevel.ToString());
#endif
            ApplicationSettingsHelper.SaveSettingsValue("ExceptionLog", stringExceptionBuilder.ToString());
        }
    }
}
