using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace VLC_WINRT_APP.Views.UserControls
{
    public sealed partial class SettingsFlyout : Windows.UI.Xaml.Controls.SettingsFlyout
    {
        public SettingsFlyout()
        {
            this.InitializeComponent();
        }
        private void VideoFolder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Flyout.ShowAttachedFlyout(sender as FrameworkElement);
        }
    }
}
