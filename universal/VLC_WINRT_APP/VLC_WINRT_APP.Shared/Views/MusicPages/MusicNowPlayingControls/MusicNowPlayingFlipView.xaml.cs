using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.ViewModels;
using System.ComponentModel;

namespace VLC_WINRT_APP.Views.MusicPages.MusicNowPlayingControls
{
    public sealed partial class MusicNowPlayingFlipView : UserControl
    {
        public MusicNowPlayingFlipView()
        {
            this.InitializeComponent();
        }

        private void MusicNowPlaying_OnLoaded(object sender, RoutedEventArgs e)
        {
            MusicNowPlaying.SelectedIndex = Locator.MediaPlaybackViewModel.TrackCollection.CurrentTrack;
            Locator.MediaPlaybackViewModel.TrackCollection.PropertyChanged += TrackCollectionOnPropertyChanged;
            this.Unloaded += MusicNowPlayingOnUnloaded;
            MusicNowPlaying.SelectionChanged += MusicNowPlaying_OnSelectionChanged;
        }

        private void MusicNowPlayingOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Locator.MediaPlaybackViewModel.TrackCollection.PropertyChanged -= TrackCollectionOnPropertyChanged;
        }

        private void TrackCollectionOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "CurrentTrack")
            {
                if (MusicNowPlaying.Items != null && (Locator.MediaPlaybackViewModel.TrackCollection.CurrentTrack == -1 || Locator.MediaPlaybackViewModel.TrackCollection.CurrentTrack < MusicNowPlaying.Items.Count))
                    MusicNowPlaying.SelectedIndex = Locator.MediaPlaybackViewModel.TrackCollection.CurrentTrack;
            }
        }

        private async void MusicNowPlaying_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Locator.MediaPlaybackViewModel.TrackCollection.CurrentTrack == -1) return;
            // TODO: Code a better way to do this >_>' This is obviously messed up (The whole thing actually)
            if (MusicNowPlaying.SelectedIndex == Locator.MediaPlaybackViewModel.TrackCollection.CurrentTrack + 1)
                await Locator.MediaPlaybackViewModel.PlayNext();
            else if (MusicNowPlaying.SelectedIndex == Locator.MediaPlaybackViewModel.TrackCollection.CurrentTrack - 1)
                await Locator.MediaPlaybackViewModel.PlayPrevious();
        }
    }
}
