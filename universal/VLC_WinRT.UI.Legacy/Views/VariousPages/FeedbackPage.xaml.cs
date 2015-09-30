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
                var result = await LogHelper.SendFeedback(fb, sendLogs);

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
                });
            }
            catch (Exception e)
            {
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    StatusTextBox.Text = "An error occured when sending the feedback.";
                    ProgressRing.IsActive = false;
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
