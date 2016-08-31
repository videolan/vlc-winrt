/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Linq;
using VLC.Database;
using VLC.Model.Music;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.MusicPlayer
{
    public class FavoriteTrackCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var track = parameter as TrackItem;
            if (track == null)
                return;
            track.Favorite = !track.Favorite;
            Locator.MediaLibrary.Update(track);
        }
    }
}
