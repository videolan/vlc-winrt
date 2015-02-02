using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Helpers;
using AppBar = CustomAppBarDesktop.AppBar;

namespace VLC_WINRT_APP.Views.UserControls.Flyouts
{
    public sealed partial class AlbumMenuFlyout : Flyout
    {
        public AlbumMenuFlyout()
        {
            this.InitializeComponent();
        }

        private void AppBar_Loaded(object sender, RoutedEventArgs e)
        {
            var appBar = AppBarHelper.SetAlbumPageButtons(new List<ICommandBarElement>());
            (sender as AppBar).PrimaryCommands = appBar;
        }
    }
}
