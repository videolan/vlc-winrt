using Windows.UI.Xaml.Controls;
using Windows.System;
using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using VLC;
using VLC.Model;
using VLC.Utils;

namespace VLC_WinRT.UI.Legacy.Views.SettingsPages
{
    public sealed partial class SettingsPageUI : Page
    {
        public SettingsPageUI()
        {
            this.InitializeComponent();
        }

        private void ApplyColorButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri($"vlc://goto/?page={nameof(VLCPage.SettingsPageUI)}"));
            App.Current.Exit();
        }
    }
}
