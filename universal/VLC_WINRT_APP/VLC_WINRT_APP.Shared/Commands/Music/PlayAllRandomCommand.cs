using System.Collections.ObjectModel;
using System.Linq;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Helpers.MusicPlayer;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;
using VLC_WINRT_APP.Views.MusicPages;
using WinRTXamlToolkit.Tools;

namespace VLC_WINRT_APP.Commands.Music
{
    public class PlayAllRandomCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (Locator.MusicLibraryVM.Tracks == null || !Locator.MusicLibraryVM.Tracks.Any()) return;
            var shuffledTracks = Locator.MusicLibraryVM.Tracks.Shuffle();
            TrackCollection trackCollection = new TrackCollection();
            await trackCollection.SetPlaylist(new ObservableCollection<TrackItem>(shuffledTracks));
            await PlayMusicHelper.AddTrackCollectionToPlaylist(trackCollection);
            if (App.ApplicationFrame.CurrentSourcePageType != typeof (MusicPlayerPage))
                App.ApplicationFrame.Navigate(typeof (MusicPlayerPage));
        }
    }
}
