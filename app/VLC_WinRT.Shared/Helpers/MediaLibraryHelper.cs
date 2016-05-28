using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VLC_WinRT.Helpers.VideoLibrary;
using VLC_WinRT.Model;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Model.Video;
using VLC_WinRT.ViewModels;
using Windows.Storage;
using Windows.Storage.Search;

namespace VLC_WinRT.Helpers
{
    public static class MediaLibraryHelper
    {
        public static async Task<IReadOnlyList<StorageFile>> GetSupportedFiles(StorageFolder folder)
        {
            IReadOnlyList<StorageFile> files = null;
            try
            {
                var queryOptions = new QueryOptions { FolderDepth = FolderDepth.Deep };
                foreach (var type in VLCFileExtensions.Supported)
                    queryOptions.FileTypeFilter.Add(type);
                var fileQueryResult = folder.CreateFileQueryWithOptions(queryOptions);
                files = await fileQueryResult.GetFilesAsync();
            }
            catch (Exception e)
            {
            }
            return files;
        }

        public static async Task<VideoItem> GetVideoItem(StorageFile file)
        {
            var media = await Locator.VLCService.GetMediaFromPath(file.Path);

            // get basic media properties
            var mP = new MediaProperties();
            mP = await Locator.VLCService.GetVideoProperties(mP, media);

            // use title decrapifier
            if (string.IsNullOrEmpty(mP?.ShowTitle))
                mP = TitleDecrapifier.tvShowEpisodeInfoFromString(mP, file.DisplayName);

            var video = new VideoItem(
                string.IsNullOrEmpty(file.DisplayName) ? file.Name : file.DisplayName,
                file.Path,
                mP.Duration,
                mP.Width,
                mP.Height,
                mP.ShowTitle,
                mP.Season,
                mP.Episode
                );

            //File = item,
            return video;
        }
    }
}
