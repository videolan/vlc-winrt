/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls.Primitives;
using VLC_WINRT.Common;
using VLC_WinRT.Helpers;
using VLC_WinRT.Helpers.MusicPlayer;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels;
using VLC_WinRT.ViewModels.MusicVM;
using VLC_WinRT.Views.MusicPages;

namespace VLC_WinRT.Commands.Music
{
    public class PlayAlbumCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (App.ApplicationFrame.CurrentSourcePageType != typeof(MusicPlayerPage))
                App.ApplicationFrame.Navigate(typeof(MusicPlayerPage));
            Locator.MusicLibraryVM.IsAlbumPageShown = false;
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
