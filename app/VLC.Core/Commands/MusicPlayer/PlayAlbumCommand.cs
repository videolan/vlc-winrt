/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using VLC.Helpers;
using VLC.Model.Music;
using VLC.ViewModels;
using VLC.Model;
using VLC.Utils;

namespace VLC.Commands.MusicPlayer
{
    public class PlayAlbumCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            AlbumItem albumItem = parameter as AlbumItem;
            if (parameter is AlbumItem)
            {
                albumItem = (AlbumItem) parameter;
            }
            else if (parameter is int)
            {
                var id = (int) parameter;
                albumItem = Locator.MediaLibrary.LoadAlbum(id);
            }

            if (albumItem != null)
            {
                var tracks = Locator.MediaLibrary.LoadTracksByAlbumId(albumItem.Id);
                await Locator.PlaybackService.SetPlaylist(tracks);
                Locator.NavigationService.Go(VLCPage.MusicPlayerPage);
            }
        }
    }
}
