/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using VLC_WinRT.Helpers;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Model;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Commands.MusicPlayer
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
                albumItem = await Locator.MediaLibrary.LoadAlbum(id);
            }

            if (albumItem != null)
            {
                Locator.NavigationService.Go(VLCPage.MusicPlayerPage);

                var tracks = await Locator.MediaLibrary.LoadTracksByAlbumId(albumItem.Id);
                await Locator.MediaPlaybackViewModel.PlaybackService.Add(tracks, false, true, tracks[0]);
            }
        }
    }
}
