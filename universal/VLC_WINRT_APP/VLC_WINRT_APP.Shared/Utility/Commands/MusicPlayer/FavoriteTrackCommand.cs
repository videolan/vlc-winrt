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
using VLC_WINRT.Utility.DataRepository;
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT_APP.ViewModels.MainPage;

namespace VLC_WINRT.Utility.Commands.MusicPlayer
{
    public class FavoriteTrackCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var track = parameter as MusicLibraryViewModel.TrackItem;
            if (track == null)
                return;
            var trackDataRepository = new TrackDataRepository();
            // searching the track in the trackcollection
            int i = Locator.MusicLibraryVM.Track.IndexOf(track);

            // If the track is favorite, then now it is not
            // if the track was not favorite, now it is
            // updating the Track collection
            Locator.MusicLibraryVM.Track[i].Favorite = !(track).Favorite;
            var trackFromArtistCollection = Locator.MusicLibraryVM.Artist.FirstOrDefault(
                x =>
                {
                    var trackItem = parameter as MusicLibraryViewModel.TrackItem;
                    return trackItem != null && x.Name == trackItem.ArtistName;
                })
                .Albums.FirstOrDefault(y =>
                {
                    var item = parameter as MusicLibraryViewModel.TrackItem;
                    return item != null && y.Name == item.AlbumName;
                })
                .Tracks.FirstOrDefault(z => z == (parameter as MusicLibraryViewModel.TrackItem));

            // Update Database
            await trackDataRepository.Update(Locator.MusicLibraryVM.Track[i]);
        }
    }
}
