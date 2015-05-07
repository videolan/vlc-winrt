/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.IO;
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
            Locator.NavigationService.GoBack_HideFlyout();
            Locator.NavigationService.Go(VLCPage.MusicPlayerPage);
            if (parameter is AlbumItem)
            {
                var album = parameter as AlbumItem;
                await PlaylistHelper.AddAlbumToPlaylist(album.Id, true, true, null, 0);
            }
        }
    }
}
