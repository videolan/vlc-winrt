/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Helpers.MusicLibrary;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.Commands.Music
{
    public class DownloadAlbumArtCommand : AlwaysExecutableCommand
    {
        public async override void Execute(object parameter)
        {
            var album = parameter as MusicLibraryVM.AlbumItem;

            if (album == null)
            {
                var args = parameter as ItemClickEventArgs;
                if (args != null)
                    album = args.ClickedItem as MusicLibraryVM.AlbumItem;
            }

            await ArtistInformationsHelper.GetAlbumPictureFromInternet(album);
        }
    }
}
