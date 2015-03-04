using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Microsoft.Xaml.Interactivity;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Views.MusicPages.MusicNowPlayingControls
{
    public sealed partial class MusicNowPlaying : UserControl
    {
        private int selectedTrack;

        public MusicNowPlaying()
        {
            this.InitializeComponent();
            this.Loaded += MusicNowPlaying_Loaded;
        }

        private void MusicNowPlaying_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Responsive();
            this.SizeChanged += MusicNowPlaying_SizeChanged;
            this.Unloaded += MusicNowPlaying_Unloaded;
        }

        private void MusicNowPlaying_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            Responsive();
        }

        private void MusicNowPlaying_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.SizeChanged -= MusicNowPlaying_SizeChanged;
        }

        private void Responsive()
        {
            if (Window.Current.Bounds.Width < 650)
            {
                VisualStateUtilities.GoToState(this, "Narrow", false);
            }
            else
            {
                VisualStateUtilities.GoToState(this, "Wide", false);
            }

            if (Window.Current.Bounds.Height < 650)
            {
                VisualStateUtilities.GoToState(this, "Tiny", false);
            }
            else
            {
                VisualStateUtilities.GoToState(this, "Tall", false);   
            }
        }

        private async void PlayPauseHold(object sender, HoldingRoutedEventArgs e)
        {
            Locator.MusicPlayerVM.PlayOrPauseCommand.Execute(null);
            await Locator.MusicPlayerVM.CleanViewModel();
            if (App.ApplicationFrame.CanGoBack)
                App.ApplicationFrame.GoBack();
        }
    }
}
