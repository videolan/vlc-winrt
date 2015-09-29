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


    }
}
