using System;
using Windows.ApplicationModel;
using Windows.Phone.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.LastFmScrobbler;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Views.VariousPages
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;

            Package thisPackage = Package.Current;
            PackageVersion version = thisPackage.Id.Version;
            string appVersion = string.Format("{0}.{1}.{2}.{3}",
                version.Major, version.Minor, version.Build, version.Revision);
            AppVersion.Text = "v" + appVersion;
        }

        private void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs backPressedEventArgs)
        {
            if (App.ApplicationFrame.CanGoBack)
                App.ApplicationFrame.GoBack();
            backPressedEventArgs.Handled = true;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
        }

        private async void ConnectToLastFM_Click(object sender, RoutedEventArgs e)
        {
            LastFmScrobblerHelper lastFm = new LastFmScrobblerHelper(App.ApiKeyLastFm, "bd9ad107438d9107296ef799703d478e");
            
            string pseudo = (string) ApplicationSettingsHelper.ReadSettingsValue("LastFmUserName");
            string pd = (string) ApplicationSettingsHelper.ReadSettingsValue("LastFmPassword");

            if (string.IsNullOrEmpty(pseudo) || string.IsNullOrEmpty(pd)) return;

            var success = await lastFm.ConnectOperation(pseudo, pd);
            if (success)
            {
                var md = new MessageDialog("Enjoy!", "You are connected to Last.FM");
                md.ShowAsync();
                Locator.SettingsVM.LastFmIsConnected = true;
            }
            else
            {
                var md = new MessageDialog("Please check your credentials and your internet connection", "We can't connect you to Last.FM");
                md.ShowAsync();
                Locator.SettingsVM.LastFmIsConnected = false;
            }
        }
    }
}
