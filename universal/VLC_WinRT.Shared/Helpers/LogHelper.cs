using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Helpers
{
    public static class LogHelper
    {
        private static StorageFile _frontendLogFile;
        private static StorageFile _backendLogFile;

        public static bool FrontendUsedForRead = false;
        private static bool frontendSignalUpdate = false;
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
                    await FileIO.WriteTextAsync(_frontendLogFile, "App updated on " + DateTime.Now.ToString());
                    frontendSignalUpdate = false;
                }
                Log("------------------------------------------");
                Log("App launch :" + DateTime.Now.ToString());
            }
            catch
            {
            }
        }

        private static async Task InitBackendFile()
        {
            // Backend file init
            _backendLogFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("BackendLog.txt", CreationCollisionOption.ReplaceExisting);
            await Locator.VLCService.PlayerInstanceReady.Task;
            Locator.VLCService.Instance.logSet(LogCb);
        }

        private static void LogCb(int param0, string param1)
        {
            Log(param1, true);
        }

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
