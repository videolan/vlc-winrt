/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WinRT.Model.Music;

namespace VLC_WinRT.Commands.Music
{
    public class DownloadAlbumArtCommand : AlwaysExecutableCommand
    {
        public async override void Execute(object parameter)
        {
            var album = parameter as AlbumItem;

            if (album == null)
            {
                var args = parameter as ItemClickEventArgs;
                if (args != null)
                    album = args.ClickedItem as AlbumItem;
            }

            await App.MusicMetaService.GetAlbumCover(album);
        }
    }
}
