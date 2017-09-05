using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using System.Threading.Tasks;
using VLC.Model.Video;
using VLC.Utils;
using VLC.ViewModels;
using Windows.Storage;
using System.Linq;
using VLC.MediaMetaFetcher;

namespace VLC.Services.RunTime
{
    public sealed class VideoMetaService
    {
        readonly VideoMDFetcher videoMdFetcher = new VideoMDFetcher(App.ApiKeyMovieDb);

        public async Task<bool> GetMovieSubtitle(VideoItem video)
        {
            if (NetworkListenerService.IsConnected && !string.IsNullOrEmpty(video.Path))
            {
                var bytes = await videoMdFetcher.GetMovieSubtitle(video);
                
                if (bytes != null)
                {
                    var success = await SaveMovieSubtitleAsync(video, bytes);
                    if (success)
                    {
                        if (video.Id > -1)
                        {
                            Locator.MediaLibrary.UpdateVideo(video);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private async Task<bool> SaveMovieSubtitleAsync(VideoItem video, byte[] sub)
        {
            if (await FetcherHelpers.SaveBytes(video.Id, "movieSub", sub, "zip", true))
            {
                var ext = await FetcherHelpers.ExtractFromArchive(video.Id, $"{ApplicationData.Current.TemporaryFolder.Path}\\{video.Id}.zip");
                if (!string.IsNullOrEmpty(ext))
                {
                    await DispatchHelper.InvokeInUIThread(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        video.IsSubtitlePreLoaded = true;
                        video.SubtitleExtension = ext;
                    });
                    return true;
                }
            }
            return false;
        }

        
        public async Task<bool> GetMoviePicture(VideoItem video)
        {
            if (NetworkListenerService.IsConnected && !string.IsNullOrEmpty(video.Name))
            {
                var bytes = await videoMdFetcher.GetMovieCover(video.Name);

                if (bytes != null)
                {
                    var success = await SaveMoviePictureAsync(video, bytes);
                    if (success)
                    {
                        Locator.MediaLibrary.UpdateVideo(video);
                        return true;
                    }
                }
            }
            return false;
        }

        private async Task<bool> SaveMoviePictureAsync(VideoItem video, byte[] img)
        {
            if (await FetcherHelpers.SaveBytes(video.Id, "moviePic", img, "jpg", false))
            {
                await DispatchHelper.InvokeInUIThread(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    video.HasMoviePicture = true;
                });
                return true;
            }
            return false;
        }
    }
}
