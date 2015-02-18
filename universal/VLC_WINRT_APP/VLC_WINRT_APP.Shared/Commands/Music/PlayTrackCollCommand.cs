using System.Linq;
using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Helpers.MusicPlayer;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;
using VLC_WINRT_APP.Views.MusicPages;

namespace VLC_WINRT_APP.Commands.Music
{
    public class PlayTrackCollCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var trackCollection = Locator.MusicLibraryVM.CurrentTrackCollection;
            if (trackCollection == null || trackCollection.Playlist == null || !trackCollection.Playlist.Any()) return;
            if (parameter is int)
            {
                await PlayMusicHelper.AddTrackCollectionToPlaylistAndPlay(trackCollection.Playlist, true, (int) parameter);
            }
            else if (parameter is ItemClickEventArgs)
            {
                var track = (TrackItem)((ItemClickEventArgs) parameter).ClickedItem;
                var index = trackCollection.Playlist.IndexOf(track);
                await PlayMusicHelper.AddTrackCollectionToPlaylistAndPlay(trackCollection.Playlist, true, index);
            }
            else await PlayMusicHelper.AddTrackCollectionToPlaylistAndPlay(trackCollection.Playlist);
            if (App.ApplicationFrame.CurrentSourcePageType != typeof (MusicPlayerPage))
                App.ApplicationFrame.Navigate(typeof (MusicPlayerPage));
        }
    }
}
