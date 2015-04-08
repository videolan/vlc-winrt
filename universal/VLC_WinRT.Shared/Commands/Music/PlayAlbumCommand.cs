/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.IO;
using VLC_WINRT.Common;
using VLC_WinRT.Helpers;
using VLC_WinRT.Helpers.MusicPlayer;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Model;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Commands.Music
{
    public class PlayAlbumCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            Locator.NavigationService.Go(VLCPage.MusicPlayerPage);
            App.RootPage.SplitShell.HideFlyout();
            try
            {
                if (parameter is AlbumItem)
                {
                    var album = parameter as AlbumItem;
                    await PlayMusicHelper.AddAlbumToPlaylist(album.Id, true, true, null, 0);
                }
            }
            catch (FileNotFoundException exception)
            {
                ToastHelper.Basic("The file doesn't exists anymore. Rebuilding the music database", true);
                LogHelper.Log("It seems that file doesn't exist anymore. Needs to rebuild Music Library: " + exception.ToString());
                return;
            }
        }
    }
}
