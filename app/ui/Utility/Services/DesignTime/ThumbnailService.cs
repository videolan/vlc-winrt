using System;
using System.Threading.Tasks;
using VLC_WINRT.Utility.Services.Interface;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace VLC_WINRT.Utility.Services.DesignTime
{
    public class ThumbnailService : IThumbnailService
    {
        public async Task<StorageItemThumbnail> GetThumbnail(StorageFile file)
        {
            var uri = new Uri("ms-appx:///Assets/Logo.png");
            StorageFile image = await StorageFile.GetFileFromApplicationUriAsync(uri);

            StorageItemThumbnail thumb = await image.GetThumbnailAsync(ThumbnailMode.VideosView);
            return thumb;
        }
    }
}