using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;
using WinRTXamlToolkit.IO.Extensions;

namespace VLC_WinRT.Helpers
{
    public static class LogHelper
    {
        private static StorageFile _frontendLogFile;
        private static StorageFile _backendLogFile;

        public static bool FrontendUsedForRead = false;
        private static bool frontendSignalUpdate;
        static readonly SemaphoreSlim WriteFileSemaphoreSlim = new SemaphoreSlim(1);

        public static StorageFile FrontEndLogFile => _frontendLogFile;
        public static StorageFile BackendLogFile => _backendLogFile;

        static LogHelper()
        {
            Task.Run(() => InitBackendFile());
        }

        private static async Task InitFrontendFile()
        {
            try
            {
                _frontendLogFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("FrontendLog.txt", CreationCollisionOption.OpenIfExists);
                if (frontendSignalUpdate)
                {
                    await FileIO.WriteTextAsync(_frontendLogFile, "App updated on " + DateTime.Now);
                    frontendSignalUpdate = false;
                }
                Log("------------------------------------------");
                Log("App launch :" + DateTime.Now);
            }
            catch
            {
            }
        }

        private static async Task InitBackendFile()
        {
            // Backend file init
            _backendLogFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("BackendLog.txt", CreationCollisionOption.OpenIfExists);
            var fileSize = await _backendLogFile.GetSize();
            if (fileSize > 50000)
            {
                _backendLogFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("BackendLog.txt", CreationCollisionOption.ReplaceExisting);
            }
            await Locator.VLCService.PlayerInstanceReady.Task;
            Locator.VLCService.Instance.logSet(LogBackendCallback);
        }

        #region loggers
        public static async void Log(object o, bool backendLog = false)
        {
            Debug.WriteLine(o.ToString());
            if (backendLog)
            {
                if (_backendLogFile == null) return;
                await WriteInLog(_backendLogFile, o.ToString());
            }
            else
            {
                if (_frontendLogFile == null)
                {
                    await InitFrontendFile();
                }
                if (_frontendLogFile == null) return;
                await WriteInLog(_frontendLogFile, o.ToString());
            }
        }

        /// <summary>
        /// Logs unhandled exceptions before the app goes down for report on next startup
        /// </summary>
        public static void Log(UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            if (unhandledExceptionEventArgs.Exception == null) return;
            Log(unhandledExceptionEventArgs.Exception);
        }

        /// <summary>
        /// Appends the <paramref name="exception"/> and all it InnerExceptions to the <paramref name="stringBuilder"/>
        /// </summary>
        public static void Log(Exception exception, string method = null)
        {
            bool inner = false;
            Log("Reporting an Exception");
            Log(Strings.MemoryUsage());

            for (Exception ex = exception; ex != null; ex = ex.InnerException)
            {
                Log(inner ? "InnerException" : "Exception:");
                Log("Message: ");
                Log(ex.Message);
                Log("HelpLink: ");
                Log(ex.HelpLink);
                Log("HResult: ");
                Log(ex.HResult.ToString());
                Log("Source: ");
                Log(ex.Source);
                Log("StackTrace: ");
                Log(ex.StackTrace);
                Log("");

                if (exception.Data != null && exception.Data.Count > 0)
                {
                    Log("Additional Data: ");
                    foreach (DictionaryEntry entry in exception.Data)
                    {
                        Log(entry.Key + ";" + entry.Value);
                    }
                    Log("");
                }

                inner = true;
            }
        }
        private static void LogBackendCallback(int param0, string param1)
        {
            Log(param1, true);
        }
        #endregion

        static async Task WriteInLog(StorageFile file, string value)
        {
            await WriteFileSemaphoreSlim.WaitAsync();
            try
            {
                if (file != null && !FrontendUsedForRead)
                {
                    await FileIO.AppendLinesAsync(file, new string[1] { value });
                }
            }
            finally
            {
                WriteFileSemaphoreSlim.Release();
            }
        }

        public static void SignalUpdate()
        {
            frontendSignalUpdate = true;
        }
    }
}
