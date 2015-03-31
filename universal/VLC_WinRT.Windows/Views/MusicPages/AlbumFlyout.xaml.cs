using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using VLC_WinRT.Helpers;
using WinRTXamlToolkit.Controls.Extensions;

namespace VLC_WinRT.Views.MusicPages
{
    public sealed partial class AlbumFlyout : SettingsFlyout
    {
        public AlbumFlyout()
        {
            this.InitializeComponent();
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

        private void AlbumFlyout_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Width = FlyoutHelper.GetSettingsFlyoutWidthFromWindowWidth(400);
        }
    }
}
