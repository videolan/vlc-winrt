using System.Threading.Tasks;
using Windows.Storage.AccessCache;
using VLC_WINRT_APP.Model.Video;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.VideoVM;
using VLC_WINRT_APP.Views.VideoPages;

namespace VLC_WINRT_APP.Helpers.VideoPlayer
{
    public static class PlayVideoHelper
    {
        public static async Task Play(this VideoItem videoVm)
        {
            if (string.IsNullOrEmpty(videoVm.Token))
            {
                string token = StorageApplicationPermissions.FutureAccessList.Add(videoVm.File);
                videoVm.Token = token;
            }
            if (App.ApplicationFrame.CurrentSourcePageType != typeof (VideoPlayerPage))
                App.ApplicationFrame.Navigate(typeof (VideoPlayerPage));
            Locator.VideoVm.CurrentVideo = videoVm;
            Locator.VideoVm.SetActiveVideoInfo(videoVm.Token);
        }
    }
}
