using System.Threading.Tasks;
using Windows.Storage.AccessCache;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.VideoVM;

namespace VLC_WINRT_APP.Helpers
{
    public static class PlayVideoHelper
    {
        public static async Task Play(this VideoVM videoVm)
        {
            if (string.IsNullOrEmpty(videoVm.Token))
            {
                string token = StorageApplicationPermissions.FutureAccessList.Add(videoVm.File);
                videoVm.Token = token;
            }
            Locator.VideoVm.CurrentVideo = videoVm;
            Locator.VideoVm.SetActiveVideoInfo(videoVm.Token);
        }
    }
}
