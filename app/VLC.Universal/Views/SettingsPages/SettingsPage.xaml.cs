using System;
using System.Linq;
using VLC.Helpers;
using VLC.Model;
using VLC.Helpers.MusicPlayer;
using VLC.Utils;
using VLC.ViewModels;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Input;
using Windows.Storage;

namespace VLC.UI.Views.SettingsPages
{
    public sealed partial class SettingsPage
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            this.Loaded += SettingsPage_Loaded;
            this.UsernameBoxLastFm.KeyUp += UsernameBoxLastFm_KeyUp;
            this.PasswordBoxLastFm.KeyUp += PasswordBoxLastFm_KeyUp;
        }

        private void UsernameBoxLastFm_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
                PasswordBoxLastFm.Focus(FocusState.Programmatic);
        }

        private void PasswordBoxLastFm_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
                ConnectToLastFM_Click(PasswordBoxLastFm, null);
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
#if STARTS
            if (DeviceHelper.GetDeviceType() != DeviceTypeEnum.Tablet)
            {
                foreach (var element in MusicSettingsPanel.Children)
                {
                    if ((string)((FrameworkElement)element).Tag == "STARTS")
                    {
                        element.Visibility = Visibility.Collapsed;
                    }
                }
                foreach (var element in VideoSettingsPanel.Children)
                {
                    if ((string)((FrameworkElement)element).Tag == "STARTS")
                    {
                        element.Visibility = Visibility.Collapsed;
                    }
                }
            }
#endif
            AppThemeSwitch.Focus(FocusState.Programmatic);
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
            App.ReloadApplicationPage();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Any())
                Locator.SettingsVM.Equalizer = e.AddedItems[0] as VLCEqualizer;
        }

        private void OnClearKeystoreClicked(object sender, RoutedEventArgs a)
        {
            Locator.SettingsVM.ClearKeystore();
        }
    }
}
