using System.Threading.Tasks;
using VLC.MediaMetaFetcher;
using VLC.Model.Video;
using VLC.Utils;
using Windows.Storage;
using VLC.Model.Library;

namespace VLC.Services.RunTime
{
    public sealed class VideoMetaService : MetaService
    {
        private readonly VideoMDFetcher _videoMdFetcher = new VideoMDFetcher(App.ApiKeyMovieDb);

        public VideoMetaService(MediaLibrary mediaLibrary, NetworkListenerService networkListenerService) 
            : base(mediaLibrary, networkListenerService)
        {
        }

        public async Task<bool> GetMovieSubtitle(VideoItem video)
        {
            if (NetworkListenerService.IsConnected && !string.IsNullOrEmpty(video.Path))
            {
                var bytes = await _videoMdFetcher.GetMovieSubtitle(video);
                
                if (bytes != null)
                {
                    var success = await SaveMovieSubtitleAsync(video, bytes);
                    if (success)
                    {
                        if (video.Id > -1)
                        {
                            MediaLibrary.UpdateVideo(video);
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
                var bytes = await _videoMdFetcher.GetMovieCover(video.Name);

                if (bytes != null)
                {
                    var success = await SaveMoviePictureAsync(video, bytes);
                    if (success)
                    {
                        MediaLibrary.UpdateVideo(video);
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
