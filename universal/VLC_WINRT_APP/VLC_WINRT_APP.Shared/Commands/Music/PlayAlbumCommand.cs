/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls.Primitives;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Helpers.MusicPlayer;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MusicPages;

namespace VLC_WINRT_APP.Commands.Music
{
    public class PlayAlbumCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            App.Transition.Edge = EdgeTransitionLocation.Right;
            if (App.ApplicationFrame.CurrentSourcePageType != typeof(MusicPlayerPage))
                App.ApplicationFrame.Navigate(typeof(MusicPlayerPage));
            Locator.MusicLibraryVM.IsAlbumPageShown = false;
            try
            {
                var album = parameter as AlbumItem;
                await PlayMusicHelper.AddAlbumToPlaylist(album.Id, true, true, null, 0);
            }
            catch (FileNotFoundException exception)
            {
                ToastHelper.Basic("The file doesn't exists anymore. Rebuilding the music database", true);
                Debug.WriteLine("It seems that file doesn't exist anymore. Needs to rebuild Music Library");
                return;
            }
        }
    }
}
