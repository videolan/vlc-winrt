using System;
using System.Collections.Generic;
using System.Text;
using VLC_WinRT.Helpers;
using VLC_WinRT.Helpers.MusicLibrary;
using VLC_WinRT.Model;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.MusicPlayer
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
                    var playlist = await Locator.MediaLibrary.LoadTracksByAlbumId(albumItem.Id);

                    await Locator.MediaPlaybackViewModel.PlaybackService.SetPlaylist(playlist, false, false, null);
                }
            }
        }
    }
}
