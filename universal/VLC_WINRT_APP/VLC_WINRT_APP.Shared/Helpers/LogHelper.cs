using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;

namespace VLC_WinRT.Helpers
{
    public static class LogHelper
    {
        public static StorageFile LogFile;
        public static bool usedForRead = false;
        private static bool signalUpdate = false;
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
        public static void Log(object o)
        {
            Debug.WriteLine(o.ToString());
            WriteInLog(o.ToString());
        }

        static void WriteInLog(string value)
        {
            if (LogFile != null && !usedForRead) FileIO.AppendTextAsync(LogFile, value);
        }

        public static void SignalUpdate()
        {
            signalUpdate = true;
        }
    }
}
