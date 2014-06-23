/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using VLC_WINRT.Common;
using VLC_WINRT.Utility.DataRepository;
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MainPage;

namespace VLC_WINRT.Utility.Commands.MusicPlayer
{
    public class FavoriteAlbumCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var album = parameter as MusicLibraryViewModel.AlbumItem;
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
            }
            else if (Locator.MusicLibraryVM.FavoriteAlbums.Contains(album))
            {
                Locator.MusicLibraryVM.FavoriteAlbums.Remove(album);
            }
            // Update database;
            await albumDataRepository.Update(album);
        }
    }
}