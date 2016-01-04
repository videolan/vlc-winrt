using System;
using System.Collections.Generic;
using System.Text;
using VLC_WinRT.Helpers;
using VLC_WinRT.Helpers.MusicLibrary;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Commands.MusicPlayer
{
    public class AddToPlayingPlaylist : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var trackItem = parameter as TrackItem;
            if (trackItem != null)
            {
                await PlaylistHelper.AddTrackToPlaylist(trackItem, false, false);
            }
            else
            {
                var albumItem = parameter as AlbumItem;
                if (albumItem != null)
                {
                    await PlaylistHelper.AddAlbumToPlaylist(albumItem.Id, false, false);
                }
            }
        }
    }
}
