using System;
using Windows.ApplicationModel;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC_WinRT.Helpers;
using VLC_WinRT.ViewModels;
using Windows.UI.Xaml.Media;
using Windows.UI;
using VLC_WinRT.SharedBackground.Helpers.MusicPlayer;

namespace VLC_WinRT.Views.VariousPages
{
    public sealed partial class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            Package thisPackage = Package.Current;
            PackageVersion version = thisPackage.Id.Version;
            string appVersion = string.Format("{0}.{1}.{2}.{3}",
                version.Major, version.Minor, version.Build, version.Revision);
            AppVersion.Text = "v" + appVersion;
            foreach(var element in RootPanel.Children)
            {
#if WINDOWS_PHONE_APP
                if((string)((FrameworkElement)element).Tag == "WindowsOnly")
                {
                    element.Visibility = Visibility.Collapsed;
                }
#endif
            }
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
            
            string pseudo = (string) ApplicationSettingsHelper.ReadSettingsValue("LastFmUserName");
            string pd = (string) ApplicationSettingsHelper.ReadSettingsValue("LastFmPassword");

            if (string.IsNullOrEmpty(pseudo) || string.IsNullOrEmpty(pd)) return;
            ErrorConnectLastFmTextBox.Text = "Connecting";
            ErrorConnectLastFmTextBox.Visibility = Visibility.Visible;
            ErrorConnectLastFmTextBox.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
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
                ErrorConnectLastFmTextBox.Text = "It didn't worked. Please check your credentials";
                Locator.SettingsVM.LastFmIsConnected = false;
            }
        }

        private void VideoFolder_Tapped(object sender, RoutedEventArgs args)
        {
            Flyout.ShowAttachedFlyout(sender as FrameworkElement);
        }
    }
}
