using System;
using Windows.ApplicationModel;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Views.VariousPages
{
    public sealed partial class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            AppVersion.Text = "v" + Strings.AppVersion;
            Extensions.HideWindowsOnlyElements(RootPanel);
            this.Loaded += SettingsPage_Loaded;
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            UserInterface.Focus(FocusState.Keyboard);
        }
    }
}
