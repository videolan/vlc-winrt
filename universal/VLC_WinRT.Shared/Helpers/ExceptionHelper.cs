using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using VLC_WinRT.Utils;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
#if WINDOWS_PHONE_APP
using Windows.ApplicationModel.Email;
using Windows.Security.ExchangeActiveSyncProvisioning;
#endif

namespace VLC_WinRT.Helpers
{
    public static class ExceptionHelper
    {
        /// <summary>
        /// Returns the current App Version
        /// </summary>
        public static string AppVersion
        {
            get
            {
                PackageVersion version = Package.Current.Id.Version;
                return String.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            }
        }

        /// <summary>
        /// Checks if any exception has been saved for reporting, prompts the user for sending it and deletes it
        /// </summary>
        public static async Task ExceptionLogCheckup()
        {
            LogHelper.FrontendUsedForRead = true;
            if (ApplicationSettingsHelper.Contains("ExceptionLog"))
            {
                var dialog = new MessageDialog(Strings.CrashReport, Strings.WeNeedYourHelp);
                dialog.Commands.Add(new UICommand(Strings.Yes, async command =>
                {
#if WINDOWS_PHONE_APP
                    EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();

                    var email = new EmailMessage();
                    email.To.Add(new EmailRecipient("modernvlc@outlook.com"));
                    email.Subject = String.Format("WP8.1 v{0} {1} ON {2}", AppVersion, deviceInfo.SystemManufacturer, deviceInfo.SystemProductName);
                    email.Body = ApplicationSettingsHelper.ReadResetSettingsValue("ExceptionLog").ToString();
                    email.Attachments.Add(new EmailAttachment(LogHelper.LogFile.Name, LogHelper.LogFile));

                    if (email.Body.Contains("GetAlbumUrl : AlbumURLWorkedViaVLC"))
                    {
                        email.Subject += " ARTCOVER";
                    }

                    await EmailManager.ShowComposeNewEmailAsync(email);
#else
                    string subject = Uri.EscapeDataString("VLC for Windows 8.1 v" + AppVersion);
                    string body = Uri.EscapeDataString(ApplicationSettingsHelper.ReadResetSettingsValue("ExceptionLog").ToString());

                    var uri = new Uri(String.Format("mailto:" + Strings.FeedbackMailAdress + "?subject={0}&body={1}", subject, body));
                    await Launcher.LaunchUriAsync(uri);
#endif
                }));
                dialog.Commands.Add(new UICommand(Strings.No, command =>
                {
                    ApplicationSettingsHelper.ReadResetSettingsValue("ExceptionLog");
                }));
                await dialog.ShowAsync();
            }
            LogHelper.FrontendUsedForRead = false;
        }

        /// <summary>
        /// Logs unhandled exceptions before the app goes down for report on next startup
        /// </summary>
        public static void UnhandledExceptionLogger(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            if (unhandledExceptionEventArgs.Exception == null) return;
            LogException(unhandledExceptionEventArgs.Exception);
        }
        
        /// <summary>
        /// Appends the <paramref name="exception"/> and all it InnerExceptions to the <paramref name="stringBuilder"/>
        /// </summary>
        public static void LogException(Exception exception, string method = null)
        {
            bool inner = false;
            LogHelper.Log("Reporting an Exception");
            AppendMemoryUsage();
            for (Exception ex = exception; ex != null; ex = ex.InnerException)
            {
                LogHelper.Log(inner ? "InnerException" : "Exception:");
                LogHelper.Log("Message: ");
                LogHelper.Log(ex.Message);
                LogHelper.Log("HelpLink: ");
                LogHelper.Log(ex.HelpLink);
                LogHelper.Log("HResult: ");
                LogHelper.Log(ex.HResult.ToString());
                LogHelper.Log("Source: ");
                LogHelper.Log(ex.Source);
                LogHelper.Log("StackTrace: ");
                LogHelper.Log(ex.StackTrace);
                LogHelper.Log("");

                if (exception.Data != null && exception.Data.Count > 0)
                {
                    LogHelper.Log("Additional Data: ");
                    foreach (DictionaryEntry entry in exception.Data)
                    {
                        LogHelper.Log(entry.Key + ";" + entry.Value);
                    }
                    LogHelper.Log("");
                }

                inner = true;
            }
        }

        /// <summary>
        /// Appends the current memory usage and limits on Windows Phone to the <paramref name="stringBuilder"/>
        /// </summary>
        private static void AppendMemoryUsage()
        {
#if WINDOWS_PHONE_APP
            try
            {
                // Gets the app's current memory usage    
                ulong AppMemoryUsageUlong = MemoryManager.AppMemoryUsage;
                // Gets the app's memory usage limit    
                ulong AppMemoryUsageLimitUlong = MemoryManager.AppMemoryUsageLimit;

                AppMemoryUsageUlong /= 1024*1024;
                AppMemoryUsageLimitUlong /= 1024*1024;
                LogHelper.Log("CurrentRAM: "); LogHelper.Log(AppMemoryUsageUlong.ToString());
                LogHelper.Log("MaxRAM: "); LogHelper.Log(AppMemoryUsageLimitUlong.ToString());
                LogHelper.Log("CommentOnRAM: "); LogHelper.Log(MemoryManager.AppMemoryUsageLevel.ToString());
            }
            catch { }
#endif
        }
    }
}
