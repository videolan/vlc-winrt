using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using VLC.Helpers.MusicLibrary;
using VLC.Model.Music;
using VLC.ViewModels;
using VLC.Utils;
using VLC.Model.Video;

namespace VLC.Helpers
{
    public class LoadImageToMemoryHelper
    {
        public static async Task LoadImageToMemory(ArtistItem item, bool thumbnail)
        {
            /*
            Normally, We would need more tight calls to try and make sure that the file
            exists in our database. However, since this is on the UI thread, we can't do that.
            Since binding images directly through XAML leads to blocked files when we
            need to delete them, we have to load them up manually. This should be enough
            of a check, for now, to make sure images load correctly.
            */
            bool fileExists = item.IsPictureLoaded;
            try
            {
                if (fileExists)
                {
                    if (thumbnail)
                    {
                        await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Low, () => item.ArtistImageThumbnail = new BitmapImage(new Uri(item.PictureThumbnail)));
                    }
                    else
                    {
                        await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Low, () => item.ArtistImage = new BitmapImage(new Uri(item.Picture)));
                    }

                    Debug.WriteLine($"Artist picture set : {item.Name}");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log("Error getting artist picture : " + item.Name);
            }

            // Failed to get the artist image or no cover image. So go online and check
            // for a new one.
            if (!fileExists)
                await Locator.MediaLibrary.FetchArtistPicOrWaitAsync(item);
        }

        public static async Task LoadImageToMemory(TrackItem item)
        {
            var albumItem = Locator.MediaLibrary.LoadAlbum(item.AlbumId);
            try
            {
                if (albumItem.IsPictureLoaded)
                {
                    await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () => item.AlbumImage = new BitmapImage(new Uri(albumItem.AlbumCoverFullUri)));
                }
                else
                {
                    await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () => item.AlbumImage = null);
                }
            }
            catch (Exception)
            {
                LogHelper.Log("Error getting album picture : " + item.Name);
            }
        }
    }
}
