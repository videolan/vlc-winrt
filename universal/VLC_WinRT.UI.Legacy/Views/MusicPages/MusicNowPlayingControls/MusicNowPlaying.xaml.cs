using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Microsoft.Xaml.Interactivity;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Views.MusicPages.MusicNowPlayingControls
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
            if (Window.Current.Bounds.Width < 640)
            {
                VisualStateUtilities.GoToState(this, "Narrow", false);
            }
            else
            {
                VisualStateUtilities.GoToState(this, "Wide", false);
            }
        }

        private async void PlayPauseHold(object sender, HoldingRoutedEventArgs e)
        {
            Locator.MediaPlaybackViewModel.PlayOrPauseCommand.Execute(null);
            await Locator.MediaPlaybackViewModel.CleanViewModel();
            Locator.NavigationService.GoBack_Default();
        }

        private void MusicNowPlaying_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                MusicNowPlayingFlipView.SelectedIndex = Locator.MediaPlaybackViewModel.TrackCollection.CurrentTrack;
                Locator.MediaPlaybackViewModel.TrackCollection.PropertyChanged += TrackCollectionOnPropertyChanged;
                this.Unloaded += MusicNowPlayingOnUnloaded;
                MusicNowPlayingFlipView.SelectionChanged += MusicNowPlaying_OnSelectionChanged;
            }
            catch { }
        }

        private void MusicNowPlayingOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Locator.MediaPlaybackViewModel.TrackCollection.PropertyChanged -= TrackCollectionOnPropertyChanged;
        }

        private void TrackCollectionOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "CurrentTrack")
            {
                if (MusicNowPlayingFlipView.Items != null && (Locator.MediaPlaybackViewModel.TrackCollection.CurrentTrack == -1 || Locator.MediaPlaybackViewModel.TrackCollection.CurrentTrack < MusicNowPlayingFlipView.Items.Count))
                    MusicNowPlayingFlipView.SelectedIndex = Locator.MediaPlaybackViewModel.TrackCollection.CurrentTrack;
            }
        }

        private async void MusicNowPlaying_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Locator.MediaPlaybackViewModel.TrackCollection.CurrentTrack == -1) return;
            // TODO: Code a better way to do this >_>' This is obviously messed up (The whole thing actually)
            if (MusicNowPlayingFlipView.SelectedIndex == Locator.MediaPlaybackViewModel.TrackCollection.CurrentTrack + 1)
                await Locator.MediaPlaybackViewModel.PlayNext();
            else if (MusicNowPlayingFlipView.SelectedIndex == Locator.MediaPlaybackViewModel.TrackCollection.CurrentTrack - 1)
                await Locator.MediaPlaybackViewModel.PlayPrevious();
        }
    }
}
