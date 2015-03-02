using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using VLC_WINRT_APP.Model.Music;
using WinRTXamlToolkit.IO.Extensions;

namespace VLC_WINRT_APP.Helpers
{
    public class LoadImageToMemoryHelper
    {
        public static async Task LoadImageToMemory(AlbumItem item)
        {
            bool fileExists = false;
            try
            {
                var fileName = string.Format("{0}.jpg", item.Id);
                fileExists = await ApplicationData.Current.LocalFolder.ContainsFolderAsync("albumPic");
                if (fileExists)
                {
                    var albumPic = await ApplicationData.Current.LocalFolder.GetFolderAsync("albumPic");
                    fileExists = await albumPic.ContainsFileAsync(fileName);
                    if (fileExists)
                    {
                        Debug.WriteLine("Opening file albumPic " + item.Id);
                        var file = await albumPic.GetFileAsync(fileName);
                        var stream = await file.OpenAsync(FileAccessMode.Read);
                        var image = new BitmapImage();
                        image.SetSource(stream);
                        stream.Dispose();
                        item.AlbumImage = image;
                    }
                }
            }
            catch (Exception)
            {
                LogHelper.Log("Error getting album picture : " + item.Name);
            }
            if (!fileExists)
                await item.LoadPicture();
        }

        public static async Task LoadImageToMemory(ArtistItem item)
        {
            bool fileExists = false;
            try
            {
                var fileName = string.Format("{0}.jpg", item.Id);
                fileExists = await ApplicationData.Current.LocalFolder.ContainsFolderAsync("artistPic");
                if (fileExists)
                {
                    var artistPic = await ApplicationData.Current.LocalFolder.GetFolderAsync("artistPic");
                    fileExists = await artistPic.ContainsFileAsync(fileName);
                    if (fileExists)
                    {
                        var file = await artistPic.GetFileAsync(fileName);
                        var stream = await file.OpenAsync(FileAccessMode.Read);
                        var image = new BitmapImage();
                        image.SetSource(stream);
                        stream.Dispose();
                        item.ArtistImage = image;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log("Error getting artist picture : " + item.Name);
            }

            // Failed to get the artist image or no cover image. So go online and check
            // for a new one.
            if (!fileExists)
                await item.LoadPicture();
        }
    }
}
