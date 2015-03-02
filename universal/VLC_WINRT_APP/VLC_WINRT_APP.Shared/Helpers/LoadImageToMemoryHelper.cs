using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Helpers
{
    public class LoadImageToMemoryHelper
    {
        public static async Task LoadImageToMemory(AlbumItem item)
        {
            var fileName = string.Format("{0}.jpg", item.Id);
            var albumPic = await ApplicationData.Current.LocalFolder.GetFolderAsync("albumPic");
            var file = await albumPic.GetFileAsync(fileName);
            var stream = await file.OpenAsync(FileAccessMode.Read);
            var image = new BitmapImage();
            image.SetSource(stream);
            stream.Dispose();
            item.AlbumImage = image;
        }
    }
}
