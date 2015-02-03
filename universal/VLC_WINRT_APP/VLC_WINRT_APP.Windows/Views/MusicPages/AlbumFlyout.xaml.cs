using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using VLC_WINRT_APP.Helpers;
using AppBar = CustomAppBarDesktop.AppBar;

namespace VLC_WINRT_APP.Views.MusicPages
{
    public sealed partial class AlbumFlyout : SettingsFlyout
    {
        public AlbumFlyout()
        {
            this.InitializeComponent();
        }

        private void AppBar_loaded(object sender, RoutedEventArgs e)
        {
            var buttons = AppBarHelper.SetAlbumPageButtons(new List<ICommandBarElement>());
            customAppBar.PrimaryCommands = buttons;
        }

        private void RootGrid_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
