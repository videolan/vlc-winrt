using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Helpers.MusicLibrary
{
    public class MusicFlow
    {
        List<ArtistItem> listArtist = new List<ArtistItem>();
        // This algorithm creates a playlist that will follow an artist, and then play similar artists, or albums of the current artist if found
        public async Task<bool> CreatePlaylist()
        {
            // First artist
            ArtistItem firstArtist = null;
            bool foundFirstArt = false;
            while (!foundFirstArt)
            {
                var artistItem = await GetFirstArtist();
                if (artistItem == null || string.IsNullOrEmpty(artistItem.Name)) return false;
                firstArtist = artistItem;
                foundFirstArt = true;
            }

            var continueFetching = true;
            while (continueFetching)
            {
                listArtist.Add(firstArtist);

                Debug.WriteLine("added " + firstArtist.Name);
                var followingArtist = await GetFollowingArtist(firstArtist);
                if (followingArtist == null)
                    continueFetching = false;
                firstArtist = followingArtist;
            }

            // done
            var tt = "";
            Debug.WriteLine("done");
            foreach (var artistItem in listArtist)
            {
                tt += artistItem.Name;
                tt += " --/-- ";
                Debug.WriteLine(artistItem.Name);
            }
            new MessageDialog(tt).ShowAsync();
            return true;
        }

        async Task<ArtistItem> GetFirstArtist()
        {
            var artistsCount = await Locator.MusicLibraryVM.MusicLibrary.ArtistCount();
            var random = new Random().Next(0, artistsCount - 1);
            var firstArtist = await Locator.MusicLibraryVM.MusicLibrary.ArtistAt(random);
            return firstArtist;
        }

        private async Task<ArtistItem> GetFollowingArtist(ArtistItem currentArtist)
        {
            // similarity
            var t = await GetFollowingArtistViaSimilarity(currentArtist);
            // music genre
            //var artistItems = await GetFollowingArtistViaGenre(currentArtist);
            // years, decades
            return null;
        }

        private async Task<List<ArtistItem>> GetFollowingArtistViaGenre(ArtistItem currentArtist)
        {
            //            if (currentArtist.Genre == null)
            //#if DEBUG
            //                currentArtist.Genre = "Rock";
            //#else 
            return null;
            //#endif
            //            var artists = await Locator.MusicMetaService.GetTopArtistGenre(currentArtist.Genre);
            //            if (artists == null || !artists.Any()) return null;
            //            var artistsInCollection = InCollection(artists.Select(x => x.Name).ToList());
            //            return artistsInCollection;
        }

        public static async Task<List<ArtistItem>> GetFollowingArtistViaSimilarity(ArtistItem currentArtist)
        {
            await Locator.MusicMetaService.GetSimilarArtists(currentArtist);
            if (currentArtist.OnlineRelatedArtists == null || !currentArtist.OnlineRelatedArtists.Any())
                return null; // no more similar artists
            var artistsInCollection = InCollection(currentArtist.OnlineRelatedArtists.Select(x => x.Name).ToList());
            return artistsInCollection;
        }

        public static async Task<List<ArtistItem>> GetPopularArtistFromGenre(string genre)
        {
            var popularArtists = await Locator.MusicMetaService.GetTopArtistGenre(genre);
            var artistsInCollection = InCollection(popularArtists.Select(x=>x.Name).ToList());
            return artistsInCollection;
        }

        static List<ArtistItem> InCollection(List<string> artistsName)
        {
            var artists = artistsName.Select(artistName => Locator.MusicLibraryVM.MusicLibrary.LoadViaArtistName(artistName).Result).Where(artistItem => artistItem != null).ToList();
            return artists;
        }
    }
}
