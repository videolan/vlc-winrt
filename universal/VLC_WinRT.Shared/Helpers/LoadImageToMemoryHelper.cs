using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using VLC_WinRT.Helpers.MusicLibrary;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Helpers
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
            bool fileExists = item.IsPictureLoaded;
            try
            {
                if (fileExists)
                {
                    var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(item.AlbumCoverFullUri));
                    using (var stream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            var image = new BitmapImage();
                            image.SetSource(stream);
                            item.AlbumImage = image;
                        });
                    }
                }
            }
            catch (Exception)
            {
                LogHelper.Log("Error getting album picture : " + item.Name);
            }
            if (!fileExists)
            {
                try
                {
                    await Locator.MusicLibraryVM.MusicLibrary.FetchAlbumCoverOrWaitAsync(item);
                }
                catch { }
            }
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
            bool fileExists = item.IsPictureLoaded;
            try
            {
                if (fileExists)
                {
                    var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(item.Picture));
                    using (var stream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            var image = new BitmapImage();
                            image.SetSource(stream);
                            item.ArtistImage = image;
                        });
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
                await Locator.MusicLibraryVM.MusicLibrary.FetchArtistPicOrWaitAsync(item);
        }

        public static async Task LoadImageToMemory(TrackItem item)
        {
            bool fileExists = !string.IsNullOrEmpty(item.Thumbnail);
            try
            {
                if (fileExists)
                {
                    var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(item.Thumbnail));
                    using (var stream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            var image = new BitmapImage();
                            image.SetSource(stream);
                            item.AlbumImage = image;
                        });
                    }
                }
            }
            catch (Exception)
            {
                LogHelper.Log("Error getting album picture : " + item.Name);
            }
        }
    }
}
