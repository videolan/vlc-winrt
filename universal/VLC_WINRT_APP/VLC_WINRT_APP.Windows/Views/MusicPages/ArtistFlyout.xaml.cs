using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using VLC_WINRT_APP.Helpers;

namespace VLC_WINRT_APP.Views.MusicPages
{
    public sealed partial class ArtistFlyout : SettingsFlyout
    {
        public ArtistFlyout()
        {
            this.InitializeComponent();
        }

        private void RootGrid_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            this.Hide();
        }

        private void ArtistFlyout_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Width = FlyoutHelper.GetSettingsFlyoutWidthFromWindowWidth(460);
        }
    }
}
