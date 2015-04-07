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
using VLC_WinRT.Helpers.MusicPlayer;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Model;

namespace VLC_WinRT.Commands.Music
{
    public class TrackClickedCommand : AlwaysExecutableCommand
    {
        public async override void Execute(object parameter)
        {
            Locator.MainVM.NavigationService.Go(VLCPage.MusicPlayerPage);
            Locator.MusicLibraryVM.IsAlbumPageShown = false;
            TrackItem track = null;
            if (parameter is ItemClickEventArgs)
            {
                var args = parameter as ItemClickEventArgs;
                track = args.ClickedItem as TrackItem;
            }
            else if (parameter is TrackItem)
            {
                track = parameter as TrackItem;
            }

            if (track == null)
            {
                // if the track is still null (for some reason), we need to break early.
                return;
            }
            await PlayMusicHelper.AddTrackToPlaylist(track, false);
        }
    }
}