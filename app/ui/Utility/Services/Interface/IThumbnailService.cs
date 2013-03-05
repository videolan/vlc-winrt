using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;

namespace VLC_WINRT.Utility.Services.Interface
{
    public interface IThumbnailService
    {
        Task<StorageItemThumbnail> GetThumbnail(StorageFile file);
    }
}