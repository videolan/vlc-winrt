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
