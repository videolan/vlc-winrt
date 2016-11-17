using libVLCX;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VLC.Helpers.VideoLibrary;
using VLC.Model;
using VLC.Model.Music;
using VLC.Model.Stream;
using VLC.Model.Video;
using VLC.ViewModels;
using Windows.Storage;
using Windows.Storage.Search;

namespace VLC.Helpers
{
    public static class MediaLibraryHelper
    {
        public static async Task ForeachSupportedFile(StorageFolder root, Func<IReadOnlyList<StorageFile>, Task> func)
        {
            Stack<StorageFolder> folders = new Stack<StorageFolder>();
            folders.Push(root);
            var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, VLCFileExtensions.Supported);

            while (folders.Count > 0)
            {
                var folder = folders.Pop();
                var fileQueryResult = folder.CreateFileQueryWithOptions(queryOptions);

                uint maxFiles = 50;
                uint filesStartIndex = 0;
                IReadOnlyList<StorageFile> files;
                do
                {
                    try
                    {
                        files = await fileQueryResult.GetFilesAsync(filesStartIndex, maxFiles);
                    }
                    catch (System.ArgumentException)
                    {
                        // Silently ignore some folders which refuse to be browsed for no apparent reasons
                        break;
                    }
                    await func(files);
                    filesStartIndex += maxFiles;
                } while (files.Count == maxFiles);

                uint maxFolders = 50;
                uint foldersStartIndex = 0;
                IReadOnlyList<StorageFolder> subFolders;
                do
                {
                    try
                    {
                        subFolders = await folder.GetFoldersAsync(CommonFolderQuery.DefaultQuery, foldersStartIndex, maxFolders);
                    }
                    catch (System.ArgumentException)
                    {
                        break;
                    }
                    foldersStartIndex += maxFolders;
                    foreach (var sf in subFolders)
                        folders.Push(sf);
                } while (subFolders.Count == maxFolders);
            }
        }

        public static async Task<VideoItem> GetVideoItem(StorageFile file)
        {
            var media = await Locator.PlaybackService.GetMediaFromPath(file.Path);
            var video = await GetVideoItem(media, string.IsNullOrEmpty(file.DisplayName) ? file.Name : file.DisplayName, file.Path);
            return video;
        }

        public static async Task<VideoItem> GetVideoItem(Media media, string name, string path)
        {
            // get basic media properties
            var mP = new MediaProperties();
            mP = await Locator.PlaybackService.GetVideoProperties(mP, media);

            // use title decrapifier
            if (string.IsNullOrEmpty(mP?.ShowTitle))
                mP = TitleDecrapifier.tvShowEpisodeInfoFromString(mP, name);

            var video = new VideoItem(
                name,
                path,
                mP.Duration,
                mP.Width,
                mP.Height,
                mP.ShowTitle,
                mP.Season,
                mP.Episode
                );
            video.IsAvailable = true;

            return video;
        }

        public static StreamMedia GetStreamItem(VLCStorageFile file)
        {
            var video = new StreamMedia();
            video.Name = file.Name;
            video.VlcMedia = file.Media;
            video.Path = file.Media.mrl();

            return video;
        }
    }
}
