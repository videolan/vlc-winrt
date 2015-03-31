using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels;
using VLC_WinRT.ViewModels.MusicVM;
using VLC_WinRT.Views.MainPages.MainMusicControls;

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
                var artistItem = GetFirstArtist();
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

        ArtistItem GetFirstArtist()
        {
            var random = new Random().Next(0, Locator.MusicLibraryVM.Artists.Count - 1);
            var firstArtist = Locator.MusicLibraryVM.Artists[random];
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
            if (currentArtist.Genre == null)
#if DEBUG
                currentArtist.Genre = "Rock";
#else 
                return null;
#endif
            var artists = await App.MusicMetaService.GetTopArtistGenre(currentArtist.Genre);
            if (artists == null || !artists.Any()) return null;
            var artistsInCollection = InCollection(artists.Select(x => x.Name).ToList());
            return artistsInCollection;
        }

        private async Task<List<ArtistItem>> GetFollowingArtistViaSimilarity(ArtistItem currentArtist)
        {
            await App.MusicMetaService.GetSimilarArtists(currentArtist);
            if (currentArtist.OnlineRelatedArtists == null || !currentArtist.OnlineRelatedArtists.Any())
                return null; // no more similar artists
            var artistsInCollection = InCollection(currentArtist.OnlineRelatedArtists.Select(x => x.Name).ToList());
            return artistsInCollection;
        }

        static List<ArtistItem> InCollection(List<string> artistsName)
        {
            return artistsName.Select(artistName => Locator.MusicLibraryVM.Artists.FirstOrDefault(x => String.Equals(x.Name, artistName, StringComparison.CurrentCultureIgnoreCase))).Where(artistItem => artistItem != null).ToList();
        }
    }
}
