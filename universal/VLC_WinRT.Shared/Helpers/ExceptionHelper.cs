using System;
using System.Collections;
using System.Collections.Generic;
using VLC_WinRT.Utils;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml;

#if WINDOWS_PHONE_APP
using Windows.Security.ExchangeActiveSyncProvisioning;
#endif

namespace VLC_WinRT.Helpers
{
    public static class ExceptionHelper
    {
        /// <summary>
        /// Checks if any exception has been saved for reporting, prompts the user for sending it and deletes it
        /// </summary>
        public static void ExceptionLogCheckup()
        {
            LogHelper.FrontendUsedForRead = true;
            RegisterForShare();
            DataTransferManager.ShowShareUI();
        }

        private static void RegisterForShare()
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(ShareFileHandler);
        }

        private static void ShareFileHandler(DataTransferManager sender, DataRequestedEventArgs e)
        {
            DataRequest request = e.Request;
            request.Data.OperationCompleted += (package, args) =>
            {
                LogHelper.FrontendUsedForRead = false;
            };

            request.Data.Properties.Title = "[" + Strings.AppVersion + "] VLC logs";
            var desc = "Please send this mail to : " + Strings.FeedbackMailAdress;
            request.Data.Properties.Description = desc;
            request.Data.SetText(desc);

#if WINDOWS_PHONE_APP
            try
            {
                EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();
                request.Data.Properties.Title += String.Format("WP8.1 v{0} {1} ON {2}", AppVersion, deviceInfo.SystemManufacturer, deviceInfo.SystemProductName);
            }
            catch
            {
            }
#endif

            DataRequestDeferral deferral = request.GetDeferral();
            try
            {
                var attachedFiles = new List<StorageFile>();
                attachedFiles.Add(LogHelper.FrontEndLogFile);
                attachedFiles.Add(LogHelper.BackendLogFile);
                request.Data.SetStorageItems(attachedFiles);
            }
            catch
            {
                deferral.Complete();
                LogHelper.FrontendUsedForRead = false;
            }
            finally
            {
                deferral.Complete();
            }
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
