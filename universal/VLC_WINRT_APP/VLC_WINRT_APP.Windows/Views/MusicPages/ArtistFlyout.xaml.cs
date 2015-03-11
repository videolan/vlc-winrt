using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

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
    }
}
