/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Linq;
using System.Threading.Tasks;
using VLC_WinRT.Database;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.MusicLibrary
{
    public class FavoriteAlbumCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var album = parameter as AlbumItem;
            if (album == null)
                return;

            album.Favorite = !album.Favorite;
            await Locator.MediaLibrary.Update(album);
            await Task.Run(() => Locator.MusicLibraryVM.RefreshRecommendedAlbums());
        }
    }
}