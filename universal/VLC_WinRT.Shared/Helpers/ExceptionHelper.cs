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
            LogHelper.usedForRead = true;
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
            LogHelper.usedForRead = false;
        }

        /// <summary>
        /// Logs unhandled exceptions before the app goes down for report on next startup
        /// </summary>
        public static void UnhandledExceptionLogger(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            StringBuilder stringExceptionBuilder = new StringBuilder("Exception Log VLC for Modern Windows v");
            stringExceptionBuilder.AppendLine(AppVersion);
            stringExceptionBuilder.Append("Message: ").AppendLine(unhandledExceptionEventArgs.Message);
            stringExceptionBuilder.Append("Date: ").AppendLine(DateTime.UtcNow.ToString("s"));
            stringExceptionBuilder.AppendLine();
            stringExceptionBuilder.Append("Current Page: ");
            if (App.ApplicationFrame != null && App.ApplicationFrame.CurrentSourcePageType != null)
            {
                stringExceptionBuilder.AppendLine(App.ApplicationFrame.CurrentSourcePageType.FullName);
            }
            else
            {
                stringExceptionBuilder.AppendLine("Null");
            }
            stringExceptionBuilder.AppendLine();

            AppendAllExceptions(stringExceptionBuilder, unhandledExceptionEventArgs.Exception);
            stringExceptionBuilder.AppendLine();
            AppendMemoryUsage(stringExceptionBuilder);

            CreateExceptionalMsg(stringExceptionBuilder.ToString());
        }

        /// <summary>
        /// Saves the <paramref name="exception"/> for report on next startup
        /// </summary>
        public static void CreateMemorizedException(string method, Exception exception)
        {
            StringBuilder stringExceptionBuilder = new StringBuilder("Exception Log VLC for Modern Windows v");
            stringExceptionBuilder.AppendLine(AppVersion);
            stringExceptionBuilder.Append("Exception at: ").AppendLine(method);
            stringExceptionBuilder.Append("Date: ").AppendLine(DateTime.UtcNow.ToString("s"));
            stringExceptionBuilder.AppendLine();

            AppendAllExceptions(stringExceptionBuilder, exception);
            stringExceptionBuilder.AppendLine();
            AppendMemoryUsage(stringExceptionBuilder);

            CreateExceptionalMsg(stringExceptionBuilder.ToString());
        }

        /// <summary>
        /// Saves the <paramref name="value"/> for report on next startup
        /// </summary>
        public static void CreateExceptionalMsg(string value)
        {
            ApplicationSettingsHelper.SaveSettingsValue("ExceptionLog", value);
        }

        /// <summary>
        /// Appends the <paramref name="exception"/> and all it InnerExceptions to the <paramref name="stringBuilder"/>
        /// </summary>
        private static void AppendAllExceptions(StringBuilder stringBuilder, Exception exception)
        {
            bool inner = false;
            for (Exception ex = exception; ex != null; ex = ex.InnerException)
            {
                stringBuilder.AppendLine(inner ? "InnerException" : "Exception:");
                stringBuilder.Append("Message: ").AppendLine(ex.Message);
                stringBuilder.Append("HelpLink: ").AppendLine(ex.HelpLink);
                stringBuilder.Append("HResult: ").AppendLine(ex.HResult.ToString());
                stringBuilder.Append("Source: ").AppendLine(ex.Source);
                stringBuilder.AppendLine("StackTrace: ");
                stringBuilder.AppendLine(ex.StackTrace);
                stringBuilder.AppendLine();

                if (exception.Data != null && exception.Data.Count > 0)
                {
                    stringBuilder.AppendLine("Additional Data: ");
                    foreach (DictionaryEntry entry in exception.Data)
                    {
                        stringBuilder.AppendLine(entry.Key + ";" + entry.Value);
                    }
                    stringBuilder.AppendLine();
                }

                inner = true;
            }
        }

        /// <summary>
        /// Appends the current memory usage and limits on Windows Phone to the <paramref name="stringBuilder"/>
        /// </summary>
        private static void AppendMemoryUsage(StringBuilder stringBuilder)
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
                stringBuilder.Append("CurrentRAM: ").AppendLine(AppMemoryUsageUlong.ToString());
                stringBuilder.Append("MaxRAM: ").AppendLine(AppMemoryUsageLimitUlong.ToString());
                stringBuilder.Append("CommentOnRAM: ").AppendLine(MemoryManager.AppMemoryUsageLevel.ToString());
            }
            catch { }
#endif
        }
    }
}
