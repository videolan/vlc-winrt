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
            MusicNowPlaying.SelectedIndex = Locator.MusicPlayerVM.TrackCollection.CurrentTrack;
            Locator.MusicPlayerVM.TrackCollection.PropertyChanged += TrackCollectionOnPropertyChanged;
            this.Unloaded += MusicNowPlayingOnUnloaded;
        }

        private void MusicNowPlayingOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Locator.MusicPlayerVM.TrackCollection.PropertyChanged -= TrackCollectionOnPropertyChanged;
        }

        private void TrackCollectionOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "CurrentTrack")
                MusicNowPlaying.SelectedIndex = Locator.MusicPlayerVM.TrackCollection.CurrentTrack;
        }

        private async void MusicNowPlaying_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Code a better way to do this >_>' This is obviously messed up (The whole thing actually)
            if (MusicNowPlaying.SelectedIndex == Locator.MusicPlayerVM.TrackCollection.CurrentTrack + 1)
                await Locator.MusicPlayerVM.PlayNext();
            else if (MusicNowPlaying.SelectedIndex == Locator.MusicPlayerVM.TrackCollection.CurrentTrack - 1)
                await Locator.MusicPlayerVM.PlayPrevious();
        }
    }
}
