/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.UI.Xaml.Controls;
using VLC.Model.Music;
using VLC.ViewModels;
using VLC.Model;
using VLC.Utils;
using VLC.Helpers;
using System.Collections.Generic;

namespace VLC.Commands.MusicPlayer
{
    public class TrackClickedCommand : AlwaysExecutableCommand
    {
        public async override void Execute(object parameter)
        {
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


            if (Locator.NavigationService.CurrentPage == VLCPage.MusicPlayerPage
                || Locator.NavigationService.CurrentPage == VLCPage.CurrentPlaylistPage)
            {
                var success = await Locator.MediaPlaybackViewModel.PlaybackService.SetPlaylist(null, false, true, track);
            }
            else
            {
                var success = await Locator.MediaPlaybackViewModel.PlaybackService.SetPlaylist(new List<IMediaItem> { track }, false, true, track);
                if (success)
                {
                    Locator.NavigationService.Go(VLCPage.MusicPlayerPage);
                }
            }
        }
    }
}