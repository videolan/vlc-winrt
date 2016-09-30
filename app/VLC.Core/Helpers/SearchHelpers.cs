using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VLC.Model;
using VLC.Model.Music;
using VLC.Model.Video;
using VLC.ViewModels;
using VLC.Model.Search;
using Windows.Storage;
using System.Collections.ObjectModel;
using VLC.Utils;

namespace VLC.Helpers
{
    public static class SearchHelpers
    {        
        public static List<AlbumItem> SearchAlbums(string tag, List<AlbumItem> results)
        {
            var albums = SearchAlbumItems(tag);
            foreach (var album in albums)
            {
                if (results != null && !results.Contains(album)) 
                    results.Add(album);
            }
            foreach (var result in results?.ToList())
            {
                if (!albums.Contains(result))
                    results.Remove(result);
            }
            return results;
        }

        public static List<VideoItem> SearchVideos(string tag, List<VideoItem> results)
        {
            var videos = SearchVideoItems(tag);
            foreach (var video in videos)
            {
                if (!results.Contains(video))
                    results.Add(video);
            }

            foreach (var result in results.ToList())
            {
                if (!videos.Contains(result))
                    results.Remove(result);
            }
            return results;
        }

        public static List<AlbumItem> SearchAlbumItems(string tag)
        {
            return Locator.MediaLibrary.Contains(nameof(AlbumItem.Name), tag);
        }

        public static List<VideoItem> SearchVideoItems(string tag)
        {
            return Locator.MediaLibrary.ContainsVideo(nameof(VideoItem.Name), tag);
        }
    }
}
