using Windows.UI.Xaml.Controls;
using Windows.System;
using System;
using VLC;
using VLC.Model;
using VLC.Utils;

namespace VLC_WinRT.UI.Legacy.Views.SettingsPages
{
    public sealed partial class SettingsPageUI : UserControl
    {
        public SettingsPageUI()
        {
            this.InitializeComponent();
       //     Extensions.HideWindowsOnlyElements(RootPanel);
        }

        private void ApplyColorButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri($"vlc://goto/?page={nameof(VLCPage.SettingsPageUI)}"));
            App.Current.Exit();
        }
    }
}
