using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Views.UserControls
{
    public sealed partial class BottomMiniPlayer : UserControl
    {
        public BottomMiniPlayer()
        {
            this.InitializeComponent();
        }

        private void RootMiniPlayer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Locator.MusicPlayerVM.GoToMusicPlayerPage.Execute(null);
        }
    }
}
