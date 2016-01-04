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
        }
    }
}
