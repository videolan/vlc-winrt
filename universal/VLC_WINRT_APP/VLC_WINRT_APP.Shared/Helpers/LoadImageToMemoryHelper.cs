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
            /*
            Normally, We would need more tight calls to try and make sure that the file
            exists in our database. However, since this is on the UI thread, we can't do that.
            Since binding images directly through XAML leads to blocked files when we
            need to delete them, we have to load them up manually. This should be enough
            of a check, for now, to make sure images load correctly.
            */
            bool fileExists = item.Picture != null;
            try
            {
                if (fileExists)
                {
                    Debug.WriteLine("Opening file albumPic " + item.Id);
                    var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(item.Picture));
                    var stream = await file.OpenAsync(FileAccessMode.Read);
                    var image = new BitmapImage();
                    stream.Seek(0);
                    image.SetSource(stream);
                    stream.Dispose();
                    item.AlbumImage = image;
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
            /*
            Normally, We would need more tight calls to try and make sure that the file
            exists in our database. However, since this is on the UI thread, we can't do that.
            Since binding images directly through XAML leads to blocked files when we
            need to delete them, we have to load them up manually. This should be enough
            of a check, for now, to make sure images load correctly.
            */
            bool fileExists = item.Picture != null;
            try
            {
                if (fileExists)
                {
                    var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(item.Picture));
                    var stream = await file.OpenAsync(FileAccessMode.Read);
                    var image = new BitmapImage();
                    stream.Seek(0);
                    image.SetSource(stream);
                    stream.Dispose();
                    item.ArtistImage = image;
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
