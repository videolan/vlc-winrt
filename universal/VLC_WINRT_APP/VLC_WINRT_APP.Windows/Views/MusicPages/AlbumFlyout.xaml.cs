using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using VLC_WINRT_APP.Helpers;
using WinRTXamlToolkit.Controls.Extensions;
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
            var s = e.OriginalSource as FrameworkElement;
            if (s != null)
            {
                var button = s.GetFirstAncestorOfType<Button>();
                if (button != null)
                {
                    var flyout = button.Flyout;
                    if (flyout != null)
                        return;
                }
            }
            this.Hide();
        }
    }
}
