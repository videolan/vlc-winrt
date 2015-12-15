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
            if (albumItem != null)
            {
            }
            else if (parameter is int)
            {
                var id = (int) parameter;
                albumItem = Locator.MusicLibraryVM.MusicLibrary.LoadAlbum(id);
            }
            if (albumItem != null)
            {
                Locator.NavigationService.Go(VLCPage.MusicPlayerPage);
                await PlaylistHelper.AddAlbumToPlaylist(albumItem.Id, true, true, null, 0);
            }
        }
    }
}
