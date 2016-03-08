using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.Utils;

namespace VLC_WinRT.UI.Legacy.Views.SettingsPages
{
    public sealed partial class SettingsPageVideo : UserControl
    {
        public SettingsPageVideo()
        {
            this.InitializeComponent();
            Extensions.HideWindowsOnlyElements(RootPanel);
        }

        private void VideoFolder_Tapped(object sender, RoutedEventArgs args)
        {
            Flyout.ShowAttachedFlyout(sender as FrameworkElement);
        }
    }
}
