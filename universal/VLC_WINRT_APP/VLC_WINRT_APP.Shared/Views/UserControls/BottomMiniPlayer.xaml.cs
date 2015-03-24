using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Microsoft.Xaml.Interactivity;
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
            Responsive();
            this.SizeChanged += BottomMiniPlayer_SizeChanged;
            this.Unloaded += BottomMiniPlayer_Unloaded;
        }

        void BottomMiniPlayer_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.SizeChanged -= BottomMiniPlayer_SizeChanged;
        }

        void BottomMiniPlayer_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            Responsive();
        }


        private void RootMiniPlayer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Locator.MusicPlayerVM.GoToMusicPlayerPage.Execute(null);
        }

        private async void PlayPauseHold(object sender, HoldingRoutedEventArgs e)
        {
            Locator.MediaPlaybackViewModel.Stop();
            await Locator.MediaPlaybackViewModel.CleanViewModel();
        }

        void Responsive()
        {
#if WINDOWS_PHONE_APP
            RootGrid.Height = 60;

#else
            RootGrid.Height = 50;
#endif
            if (this.ActualWidth > 700)
                VisualStateUtilities.GoToState(this, "FullWindows", false);
            else if (this.ActualWidth > 400)
                VisualStateUtilities.GoToState(this, "Narrow", false);
            else if (this.ActualWidth > 200)
                VisualStateUtilities.GoToState(this, "ExtraNarrow", false);
            else
                VisualStateUtilities.GoToState(this, "Minimum", false);
        }
    }
}
