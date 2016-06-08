using System;
using VLC_WinRT.Helpers;
using VLC_WinRT.Model;
using VLC_WinRT.SharedBackground.Helpers.MusicPlayer;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace VLC_WinRT.UI.UWP.Views.SettingsPages
{
    public sealed partial class SettingsPage
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }
        
        void FocusTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Locator.MainVM.KeyboardListenerService.CanListen = true;
        }

        void FocusTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Locator.MainVM.KeyboardListenerService.CanListen = false;
        }

        private async void ConnectToLastFM_Click(object sender, RoutedEventArgs e)
        {
            LastFMScrobbler lastFm = new LastFMScrobbler(App.ApiKeyLastFm, "bd9ad107438d9107296ef799703d478e");

            string pseudo = (string)ApplicationSettingsHelper.ReadSettingsValue("LastFmUserName");
            string pd = (string)ApplicationSettingsHelper.ReadSettingsValue("LastFmPassword");

            if (string.IsNullOrEmpty(pseudo) || string.IsNullOrEmpty(pd)) return;
            ErrorConnectLastFmTextBox.Text = Strings.Connecting;
            ErrorConnectLastFmTextBox.Visibility = Visibility.Visible;
            ErrorConnectLastFmTextBox.Foreground = new SolidColorBrush(Colors.Gray);
            var success = await lastFm.ConnectOperation(pseudo, pd);
            if (success)
            {
                ErrorConnectLastFmTextBox.Text = "";
                ErrorConnectLastFmTextBox.Visibility = Visibility.Collapsed;
                Locator.SettingsVM.LastFmIsConnected = true;
            }
            else
            {
                ErrorConnectLastFmTextBox.Foreground = new SolidColorBrush(Colors.Red);
                ErrorConnectLastFmTextBox.Text = Strings.CheckCredentials;
                Locator.SettingsVM.LastFmIsConnected = false;
            }
        }

        private void VideoFolder_Tapped(object sender, RoutedEventArgs args)
        {
            Flyout.ShowAttachedFlyout(sender as FrameworkElement);
        }

        private void ForceRefreshLanguage(object sender, RoutedEventArgs e)
        {
            Locator.NavigationService.RefreshCurrentPage();
        }

        private void ApplyColorButton_Click(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri($"vlc://goto/?page={nameof(VLCPage.SettingsPageUI)}"));
            App.Current.Exit();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Locator.SettingsVM.Equalizer = e.AddedItems[0] as VLCEqualizer;
        }
    }
}
