using System.Diagnostics;
using Windows.Devices.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Microsoft.Xaml.Interactivity;
using VLC_WinRT.ViewModels;
using Windows.UI.Xaml;
using VLC_WinRT.Helpers;

namespace VLC_WinRT.Views.UserControls
{
    public sealed partial class BottomMiniPlayer : UserControl
    {
        public BottomMiniPlayer()
        {
            InitializeComponent();
            Loaded += BottomMiniPlayer_Loaded;
        }

        void BottomMiniPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            Responsive();
            Window.Current.SizeChanged += Current_SizeChanged;
            Unloaded += BottomMiniPlayer_Unloaded;
        }

        void BottomMiniPlayer_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Current_SizeChanged;
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
            if (Window.Current.Bounds.Width > 1150)
                VisualStateUtilities.GoToState(this, "FullWindows", false);
            else if (Window.Current.Bounds.Width > 900)
                VisualStateUtilities.GoToState(this, "Narrow", false);
            else if (Window.Current.Bounds.Width > 200)
                VisualStateUtilities.GoToState(this, "ExtraNarrow", false);
            else
                VisualStateUtilities.GoToState(this, "Minimum", false);
        }
    }
}
