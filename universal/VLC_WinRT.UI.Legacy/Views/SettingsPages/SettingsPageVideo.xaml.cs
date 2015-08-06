using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
namespace VLC_WinRT.UI.Legacy.Views.SettingsPages
{
    public sealed partial class SettingsPageVideo : UserControl
    {
        public SettingsPageVideo()
        {
            this.InitializeComponent();
        }
        private void VideoFolder_Tapped(object sender, RoutedEventArgs args)
        {
            Flyout.ShowAttachedFlyout(sender as FrameworkElement);
        }
    }
}
