/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Linq;
using VLC_WinRT.Database;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.MusicPlayer
{
    public class FavoriteTrackCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var track = parameter as TrackItem;
            if (track == null)
                return;
            var trackDatabase = new TrackDatabase();
            // searching the track in the trackcollection
            int i = Locator.MusicLibraryVM.MusicLibrary.Tracks.IndexOf(track);

            // If the track is favorite, then now it is not
            // if the track was not favorite, now it is
            // updating the Track collection
            Locator.MusicLibraryVM.MusicLibrary.Tracks[i].Favorite = !(track).Favorite;
            // Update Database
            await trackDatabase.Update(Locator.MusicLibraryVM.MusicLibrary.Tracks[i]);
        }
    }
}
