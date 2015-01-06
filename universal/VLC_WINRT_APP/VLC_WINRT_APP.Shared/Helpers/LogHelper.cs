using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;

namespace VLC_WINRT_APP.Helpers
{
    public static class LogHelper
    {
        private static StorageFile LogFile;

        static LogHelper()
        {
            Initialize();
        }

        static async Task Initialize()
        {
            LogFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("LogFile.txt", CreationCollisionOption.ReplaceExisting);
        }
        public static void Log(object o)
        {
            Debug.WriteLine(o.ToString());
            WriteInLog(o.ToString());
        }

        static void WriteInLog(string value)
        {
            if (LogFile != null) FileIO.WriteTextAsync(LogFile, value);
        }
    }
}
