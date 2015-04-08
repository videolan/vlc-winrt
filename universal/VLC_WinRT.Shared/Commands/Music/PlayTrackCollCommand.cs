using System.Linq;
using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WinRT.Helpers.MusicPlayer;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Model;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Commands.Music
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
            Locator.NavigationService.Go(VLCPage.MusicPlayerPage);
        }
    }
}
