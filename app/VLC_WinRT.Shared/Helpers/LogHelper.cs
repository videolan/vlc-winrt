using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.Web.Http;
using VLC_WinRT.Model;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;
using WinRTXamlToolkit.IO.Extensions;
using HttpClient = Windows.Web.Http.HttpClient;
using Windows.Web.Http.Headers;

namespace VLC_WinRT.Helpers
{
    public static class LogHelper
    {
        private static StorageFile _frontendLogFile;
        private static StorageFile _backendLogFile;

        private static readonly List<string> backEndBuffer = new List<string>(5);
        private static readonly List<string> frontEndBuffer = new List<string>(5);

        static readonly SemaphoreSlim BackendSemaphoreSlim = new SemaphoreSlim(1);
        static readonly SemaphoreSlim FrontendSemaphoreSlim = new SemaphoreSlim(1);

        static LogHelper()
        {
            Task.Run(() => InitBackendFile());
            Task.Run(() => InitFrontendFile());
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                args.SetObserved();
                ApplicationSettingsHelper.SaveSettingsValue("LastUnhandledException", StringsHelper.ExceptionToString(args.Exception));
                var lines = new List<string>(2)
                {
                    "Unobserved Task Exception:",
                    args.Exception.ToString()
                };
                Task.Run(() => FrontendSemaphore(() => WriteInLog(_frontendLogFile, lines)));
            };

            App.Current.UnhandledException += (o, ex) =>
            {
                ApplicationSettingsHelper.SaveSettingsValue("LastUnhandledException", StringsHelper.ExceptionToString(ex.Exception));
                var lines = new List<string>(2)
                {
                    "Unhandled Exception:",
                    ex.Exception.ToString()
                };
                Task.Run(() => FrontendSemaphore(() => WriteInLog(_frontendLogFile, lines)));
            };
        }

        private static async Task InitFrontendFile()
        {
            try
            {
                _frontendLogFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("FrontendLog.txt", CreationCollisionOption.ReplaceExisting);

                Log("------------------------------------------");
                Log("App launch :" + DateTime.Now);

                if (ApplicationSettingsHelper.Contains("LastUnhandledException"))
                {
                    Log("Found previous unhandled exception");
                    var unhandledEx = ApplicationSettingsHelper.ReadResetSettingsValue("LastUnhandledException");
                    Log(unhandledEx.ToString());
                }
            }
            catch
            {
            }
        }

        private static async Task InitBackendFile()
        {
            // Backend file init
            _backendLogFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("BackendLog.txt", CreationCollisionOption.OpenIfExists);
            await Locator.VLCService.PlayerInstanceReady.Task;
            Locator.VLCService.Instance.logSet(LogBackendCallback);
        }

        public static async Task ResetBackendFile()
        {
            await Task.Run(() => BackendSemaphore(async () => await ResetBackendLogFile()));
        }

        #region loggers

        public static void Log(string s, bool backend = false)
        {
            if (string.IsNullOrEmpty(s)) return;
            try
            {
                //Debug.WriteLine(s);
                if (backend)
                {
                    backEndBuffer.Add(s);
                    if (backEndBuffer.Count >= 5)
                    {
                        var lines = backEndBuffer.ToList();
                        backEndBuffer.Clear();
                        Task.Run(() => BackendSemaphore(() => WriteInLog(_backendLogFile, lines)));
                    }
                }
                else
                {
                    frontEndBuffer.Add(s);
                    if (frontEndBuffer.Count >= 5)
                    {
                        var lines = frontEndBuffer.ToList();
                        frontEndBuffer.Clear();
                        Task.Run(() => FrontendSemaphore(() => WriteInLog(_frontendLogFile, lines)));
                    }
                }
            }
            catch { }
        }
        
        
        private static void LogBackendCallback(int param0, string param1)
        {
            Log(param1, true);
        }
        #endregion

        static async Task WriteInLog(StorageFile file, List<string> values)
        {
            if (file != null)
            {
                await FileIO.AppendLinesAsync(file, values);
            }
        }

        static async Task ResetBackendLogFile()
        {
            _backendLogFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("BackendLog.txt", CreationCollisionOption.ReplaceExisting);
        }

        static async Task BackendSemaphore(Func<Task> a)
        {
            await BackendSemaphoreSlim.WaitAsync();
            try
            {
                await a();
            }
            finally
            {
                BackendSemaphoreSlim.Release();
            }
        }

        static async Task FrontendSemaphore(Func<Task> a)
        {
            await FrontendSemaphoreSlim.WaitAsync();
            try
            {
                await a();
            }
            finally
            {
                FrontendSemaphoreSlim.Release();
            }
        }

        public static async Task<Windows.Web.Http.HttpResponseMessage> SendFeedback(Feedback fb, bool sendLogs)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new HttpMediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("X-ZUMO-APPLICATION", "SBEhmMRzWBrKTGXfDkhVNGfXmsSrzv88");

            if (sendLogs)
            {
                fb.BackendLog = await FileIO.ReadTextAsync(_backendLogFile) ?? "None";
                fb.FrontendLog = await FileIO.ReadTextAsync(_frontendLogFile) ?? "None";
            }
            else
            {
                fb.BackendLog = fb.FrontendLog = "None";
            }

            var jsonSer = new DataContractJsonSerializer(typeof(Feedback));
            var ms = new MemoryStream();
            jsonSer.WriteObject(ms, fb);
            ms.Position = 0;
            var sr = new StreamReader(ms);
            var theContent = new StringContent(sr.ReadToEnd(), System.Text.Encoding.UTF8, "application/json");

            var str = await theContent.ReadAsStringAsync();
            var result = await httpClient.PostAsync(new Uri(Strings.FeedbackAzureURL), new HttpStringContent(str, UnicodeEncoding.Utf8, "application/json"));
            return result;
        }
    }
}