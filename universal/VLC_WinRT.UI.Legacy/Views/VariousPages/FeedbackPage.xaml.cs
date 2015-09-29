using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using VLC_WinRT.Helpers;
using VLC_WinRT.Model;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;
using WinRTXamlToolkit.Controls.Extensions;
using HttpClient = Windows.Web.Http.HttpClient;

namespace VLC_WinRT.UI.Legacy.Views.VariousPages
{
    public sealed partial class FeedbackPage
    {
        public FeedbackPage()
        {
            this.InitializeComponent();
        }

        private void SendFeedback_Click(object sender, RoutedEventArgs e)
        {
            LogHelper.FrontendUsedForRead = true;
            var fbItem = new Feedback();

            if (InsiderCheckBox.IsChecked.HasValue && InsiderCheckBox.IsChecked.Value)
            {
                if (string.IsNullOrEmpty(BuildNumberTextBox.Text))
                {
                    StatusTextBox.Text = "Please specify the Windows Insider build number";
                    return;
                }
                int buildN;
                if (int.TryParse(BuildNumberTextBox.Text, out buildN) && buildN > 10000 && buildN < 11000) // UGLY but should do the trick for now
                {
                    fbItem.PlatformBuild = buildN;
                }
                else
                {
                    StatusTextBox.Text = "The Windows Insider build number is incorrect";
                    return;
                }
            }

            fbItem.Comment = DetailsTextBox.Text;
            fbItem.Summary = SummaryTextBox.Text;

            var sendLogs = SendLogsCheckBox.IsChecked.HasValue && SendLogsCheckBox.IsChecked.Value;
            StatusTextBox.Text = "Sending feedback ...";
            ProgressRing.IsActive = true;
            Task.Run(() => SendFeedbackItem(fbItem, sendLogs));
        }

        public async Task SendFeedbackItem(Feedback fb, bool sendLogs)
        {
            try
            {
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Accept.Add(new HttpMediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("X-ZUMO-APPLICATION", "SBEhmMRzWBrKTGXfDkhVNGfXmsSrzv88");

                if (sendLogs)
                {
                    fb.BackendLog = await FileIO.ReadTextAsync(LogHelper.BackendLogFile) ?? "None";
                    fb.FrontendLog = await FileIO.ReadTextAsync(LogHelper.FrontEndLogFile) ?? "None";
                }
                else
                {
                    fb.BackendLog = fb.FrontendLog = "None";
                }

                var jsonSer = new DataContractJsonSerializer(typeof (Feedback));
                var ms = new MemoryStream();
                jsonSer.WriteObject(ms, fb);
                ms.Position = 0;
                var sr = new StreamReader(ms);
                var theContent = new StringContent(sr.ReadToEnd(), System.Text.Encoding.UTF8, "application/json");

                var str = await theContent.ReadAsStringAsync();
                var result = await httpClient.PostAsync(new Uri(Strings.FeedbackAzureURL), new HttpStringContent(str, UnicodeEncoding.Utf8, "application/json"));

                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    if (result.EnsureSuccessStatusCode().IsSuccessStatusCode)
                    {
                        Locator.NavigationService.Go(VLCPage.SettingsPage);
                        ToastHelper.Basic("Feedback sent ! Thank you.");
                    }
                    else
                    {
                        StatusTextBox.Text = "An error occured when sending the feedback.";
                        ProgressRing.IsActive = false;
#if DEBUG
                        var md = new MessageDialog(result.ReasonPhrase + " - " + result.Content + " - " + result.StatusCode, "Bug in the Request");
                        await md.ShowAsyncQueue();
#endif
                    }
                    LogHelper.FrontendUsedForRead = false;
                });
            }
            catch (Exception e)
            {
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    StatusTextBox.Text = "An error occured when sending the feedback.";
                    ProgressRing.IsActive = false;
                    LogHelper.FrontendUsedForRead = false;
#if DEBUG
                    var md = new MessageDialog(e.ToString(), "Bug");
                    await md.ShowAsyncQueue();
#endif
                });
            }
        }

        private void InsiderCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            BuildNumberTextBox.Visibility = Visibility.Visible;
        }

        private void InsiderCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            BuildNumberTextBox.Visibility = Visibility.Collapsed;
        }
    }
}
