using System;
using System.Collections.Generic;
using System.Text;
using VLC.Helpers;
using VLC.Helpers.MusicLibrary;
using VLC.Model;
using VLC.Model.Music;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.MusicPlayer
{
    public class AddToPlayingPlaylist : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var trackItem = parameter as TrackItem;
            if (trackItem != null)
            {
                var playlist = new List<IMediaItem>() { trackItem };

                await Locator.MediaPlaybackViewModel.PlaybackService.SetPlaylist(playlist, false, false, null);
            }
            else
            {
                var albumItem = parameter as AlbumItem;
                if (albumItem != null)
                {
                    var playlist = Locator.MediaLibrary.LoadTracksByAlbumId(albumItem.Id);

                    await Locator.MediaPlaybackViewModel.PlaybackService.SetPlaylist(playlist, false, false, null);
                }
            }
        }
    }
}
