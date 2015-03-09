/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Linq;
using VLC_WINRT.Common;
using VLC_WINRT_APP.DataRepository;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Commands.Music
{
    public class FavoriteAlbumCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var album = parameter as AlbumItem;
            if (album == null)
                return;
            // If the album is favorite, then now it is not
            // if the album was not favorite, now it is
            album.Favorite = !album.Favorite;
            var albumDataRepository = new AlbumDataRepository();
            // updating the FavoriteAlbums collection
            if (album.Favorite)
            {
                Locator.MusicLibraryVM.FavoriteAlbums.Add(album);
                // if the album is already in the list then don't add it, simply
                if (Locator.MusicLibraryVM.RandomAlbums.FirstOrDefault(x => x.Id == album.Id) == null)
                    Locator.MusicLibraryVM.RandomAlbums.Add(album);
            }
            else if (Locator.MusicLibraryVM.FavoriteAlbums.Contains(album))
            {
                Locator.MusicLibraryVM.FavoriteAlbums.Remove(album);
                Locator.MusicLibraryVM.RandomAlbums.Remove(album);
            }
            // Update database;
            await albumDataRepository.Update(album);
        }
    }
}