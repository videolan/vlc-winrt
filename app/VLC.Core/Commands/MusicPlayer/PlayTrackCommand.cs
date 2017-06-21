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
    public class PlayTrackCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            TrackItem track = null;
            if (parameter is ItemClickEventArgs)
            {
                var itemClickEventArgs = (ItemClickEventArgs) parameter;
                track = itemClickEventArgs.ClickedItem as TrackItem;
            }
            else if (parameter is TrackItem)
            {
                track = (TrackItem) parameter;
            }

            if (track == null)
            {
                // if the track is still null (for some reason), we need to break early.
                return;
            }


            if (Locator.NavigationService.CurrentPage == VLCPage.MusicPlayerPage
                || Locator.NavigationService.CurrentPage == VLCPage.CurrentPlaylistPage)
            {
                Locator.PlaybackService.SetPlaylistMedia(track);
            }
            else
            {
                await Locator.PlaybackService.SetPlaylist(new List<IMediaItem> { track });
            }
        }
    }
}