using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace VLC_WinRT.Helpers
{
    public static class LogHelper
    {
        public static StorageFile LogFile;
        public static bool usedForRead = false;
        private static bool signalUpdate = false;
        static readonly SemaphoreSlim WriteFileSemaphoreSlim = new SemaphoreSlim(1);
        static LogHelper()
        {
            Initialize();
        }

        static async Task Initialize()
        {
            LogFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("LogFile.txt", CreationCollisionOption.OpenIfExists);
            if (signalUpdate)
            {
                await FileIO.WriteTextAsync(LogFile, "App updated on " + DateTime.Now.ToString());
                signalUpdate = false;
            }
            Log("------------------------------------------");
            Log("------------------------------------------");
            Log("------------------------------------------");
            Log("App launch " + DateTime.Now.ToString());
        }

        public static async void Log(object o)
        {
            Debug.WriteLine(o.ToString());            
            await WriteInLog(LogFile, o.ToString());
        }

        public static void RuntimeLog(object o)
        {
            Debug.WriteLine(o.ToString());
        }

        static async Task WriteInLog(StorageFile file, string value)
        {
            await WriteFileSemaphoreSlim.WaitAsync();
            try
            {
                if(file != null && !usedForRead)
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
            signalUpdate = true;
        }
    }
}
