using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Microsoft.Xaml.Interactivity;
using VLC_WinRT.ViewModels;
using Windows.UI.Xaml;

namespace VLC_WinRT.Views.UserControls
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
            Window.Current.SizeChanged += Current_SizeChanged;
            this.Unloaded += BottomMiniPlayer_Unloaded;
        }

        void BottomMiniPlayer_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Current_SizeChanged;
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

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Responsive();
        }

        void Responsive()
        {
            if (this.ActualWidth > 700)
                VisualStateUtilities.GoToState(this, "FullWindows", false);
            else if (this.ActualWidth > 500)
                VisualStateUtilities.GoToState(this, "Narrow", false);
            else if (this.ActualWidth > 200)
                VisualStateUtilities.GoToState(this, "ExtraNarrow", false);
            else
                VisualStateUtilities.GoToState(this, "Minimum", false);

            if (Window.Current.Bounds.Height < 700)
                RootGrid.Height = 60;
            else
                RootGrid.Height = 75;
        }
    }
}
