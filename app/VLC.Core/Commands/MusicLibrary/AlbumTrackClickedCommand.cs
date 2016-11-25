using Windows.UI.Xaml.Controls;
using VLC.Model.Music;
using VLC.ViewModels;
using VLC.Model;
using VLC.Utils;
using VLC.Helpers;
using System.Threading.Tasks;

namespace VLC.Commands.MusicLibrary
{
    public class AlbumTrackClickedCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            Locator.NavigationService.GoOnPlaybackStarted(VLCPage.MusicPlayerPage);
            TrackItem track = null;
            if (parameter is ItemClickEventArgs)
            {
                var args = parameter as ItemClickEventArgs;
                track = args.ClickedItem as TrackItem;
            }
            if (track == null)
            {
                // if the track is still null (for some reason), we need to break early.
                return;
            }

            var playlist = Locator.MediaLibrary.LoadTracksByAlbumId(track.AlbumId);
            var item = playlist.Find((t) => t.Id == track.Id);
            if (item == null)
                return;
            var idx = playlist.IndexOf(item);
            await Locator.PlaybackService.SetPlaylist(playlist, idx);
        }
    }
}
