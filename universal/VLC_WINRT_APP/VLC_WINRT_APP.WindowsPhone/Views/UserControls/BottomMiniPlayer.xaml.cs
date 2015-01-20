using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Helpers;

namespace VLC_WINRT_APP.Views.UserControls
{
    public sealed partial class BottomMiniPlayer : UserControl
    {
        public BottomMiniPlayer()
        {
            this.InitializeComponent();
            this.Loaded += BottomMiniPlayer_Loaded;
        }

        void BottomMiniPlayer_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.SizeChanged += BottomMiniPlayer_SizeChanged;
            this.Unloaded += BottomMiniPlayer_Unloaded;
        }

        void BottomMiniPlayer_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.SizeChanged -= BottomMiniPlayer_SizeChanged;
        }

        void BottomMiniPlayer_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            if (DisplayHelper.IsPortrait())
            {
                RootGrid.Height = 80;
            }
            else
            {
                RootGrid.Height = 0;
            }
        }


        private void RootMiniPlayer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Locator.MusicPlayerVM.GoToMusicPlayerPage.Execute(null);
        }

        private async void PlayPauseHold(object sender, HoldingRoutedEventArgs e)
        {
            Locator.MusicPlayerVM.Stop();
            await Locator.MusicPlayerVM.CleanViewModel();
        }
    }
}
