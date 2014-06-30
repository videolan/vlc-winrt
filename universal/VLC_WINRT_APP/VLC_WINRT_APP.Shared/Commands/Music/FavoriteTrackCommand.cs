/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Linq;
using VLC_WINRT.Common;
using VLC_WINRT_APP.DataRepository;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.Commands.MusicPlayer
{
    public class FavoriteTrackCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var track = parameter as MusicLibraryVM.TrackItem;
            if (track == null)
                return;
            var trackDataRepository = new TrackDataRepository();
            // searching the track in the trackcollection
            int i = Locator.MusicLibraryVM.Tracks.IndexOf(track);

            // If the track is favorite, then now it is not
            // if the track was not favorite, now it is
            // updating the Track collection
            Locator.MusicLibraryVM.Tracks[i].Favorite = !(track).Favorite;
            var trackFromArtistCollection = Locator.MusicLibraryVM.Artists.FirstOrDefault(
                x =>
                {
                    var trackItem = parameter as MusicLibraryVM.TrackItem;
                    return trackItem != null && x.Name == trackItem.ArtistName;
                })
                .Albums.FirstOrDefault(y =>
                {
                    var item = parameter as MusicLibraryVM.TrackItem;
                    return item != null && y.Name == item.AlbumName;
                })
                .Tracks.FirstOrDefault(z => z == (parameter as MusicLibraryVM.TrackItem));

            // Update Database
            await trackDataRepository.Update(Locator.MusicLibraryVM.Tracks[i]);
        }
    }
}
