using libVLCX;
using System;
using System.Collections.Generic;
using System.Text;
using VLC.Model;
using VLC.Model.FileExplorer;
using System.Threading.Tasks;
using VLC.Utils;
using Windows.UI.Core;
using System.Linq;
using System.Diagnostics;
using VLC.Model.Video;
using VLC.Model.Stream;
using VLC.Helpers;
using Windows.Foundation;

namespace VLC.ViewModels.Others.VlcExplorer
{
    public class VLCFileExplorerViewModel : FileExplorer
    {
        public VLCFileExplorerViewModel(Media media, RootFolderType ftype)
            : base(media.meta(MediaMeta.Title), ftype)
        {
            BackStack.Add(new VLCStorageFolder(media));
            var mrl = media.mrl();
            var schemeEnd = mrl.IndexOf("://");
            if (schemeEnd > -1)
            {
                var scheme = mrl.Substring(0, schemeEnd);
                base.RootMediaType = scheme;
            }
        }

        public override async Task GetFiles()
        {
            try
            {
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
                {
                    StorageItems.Clear();
                    IsFolderEmpty = false;
                    IsLoadingFiles = true;
                });
                var currentMedia = BackStack.Last().Media;
                if (currentMedia == null)
                    return;
                var mediaList = await Locator.MediaLibrary.DiscoverMediaList(currentMedia);
                for (int i = 0; i < mediaList.count(); i++)
                {
                    var media = mediaList.itemAtIndex(i);
                    IVLCStorageItem storageItem = null;
                    if (media.type() == MediaType.Directory)
                    {
                        storageItem = new VLCStorageFolder(media);
                    }
                    else if (media.type() == MediaType.File)
                    {
                        storageItem = new VLCStorageFile(media);
                    }
                    if (storageItem == null) return;
                    await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () => StorageItems.Add(storageItem));
                }
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
                {
                    OnPropertyChanged(nameof(StorageItems));
                    IsFolderEmpty = !StorageItems.Any();
                    IsLoadingFiles = false;
                });
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception when getting network files {e.ToString()}");
            }
        }
        
        public override async Task NavigateTo(IVLCStorageItem storageItem)
        {
            var item = storageItem as VLCStorageFolder;
            if (item != null)
            {
                BackStack.Add(item);
                await GetFiles();
            }
            else
            {
                var file = storageItem as VLCStorageFile;
                // TODO : Difference between audio and video, here ? Hint: i don't think so
                var video = MediaLibraryHelper.GetStreamItem(file);
                await Locator.PlaybackService.SetPlaylist(new List<IMediaItem> { video });
            }
            OnPropertyChanged(nameof(CurrentFolderName));
        }
    }
}
