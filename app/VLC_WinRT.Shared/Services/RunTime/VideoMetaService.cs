using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VLC_WinRT.MediaMetaFetcher;
using VLC_WinRT.Model.Video;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Services.RunTime
{
    public sealed class VideoMetaService
    {
        readonly VideoMDFetcher videoMdFetcher = new VideoMDFetcher(App.ApiKeyMovieDb);

        public async Task<bool> GetMoviePicture(VideoItem video)
        {
            if (Locator.MainVM.IsInternet && !string.IsNullOrEmpty(video.Name))
            {
                var bytes = await videoMdFetcher.GetMovieCover(video.Name);

                if (bytes != null)
                {
                    var success = await SaveMoviePictureAsync(video, bytes);
                    if (success)
                    {
                        await Locator.MediaLibrary.UpdateVideo(video);
                        return true;
                    }
                }
            }
            return false;
        }

        private async Task<bool> SaveMoviePictureAsync(VideoItem video, byte[] img)
        {
            if (await FetcherHelpers.SaveImage(video.Id, "moviePic", img))
            {
                await DispatchHelper.InvokeAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    video.IsPictureLoaded = true;
                    video.HasMoviePicture = true;
                });
                await video.ResetVideoPicture();
                return true;
            }
            return false;
        }
    }
}
