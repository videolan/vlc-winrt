using libVLCX;
using System;
using System.Collections.Generic;
using System.Text;
using VLC_WinRT.Model;
using VLC_WinRT.Model.FileExplorer;
using System.Threading.Tasks;
using VLC_WinRT.Utils;
using Windows.UI.Core;
using System.Linq;

namespace VLC_WinRT.ViewModels.Others.VlcExplorer
{
    public class VLCFileExplorerViewModel : FileExplorer
    {
        public VLCFileExplorerViewModel(Media media, RootFolderType ftype)
            : base(media.meta(MediaMeta.Title), ftype)
        {
            BackStack.Add(new VLCStorageFolder(media));
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
                Locator.VLCService.OnSDItemAdded += VLCService_OnSDItemAdded;
                Locator.VLCService.DiscoverMediaList(currentMedia);
            }
            catch (Exception e)
            {

            }
        }

        private void VLCService_OnSDItemAdded(Media media, bool root)
        {
            if (!root)
                StorageItems.Add(new VLCStorageFile(media));
        }

        public override async Task NavigateTo(IVLCStorageItem storageItem)
        {
            var item = storageItem as VLCStorageFolder;
            if (item != null)
            {
                BackStack.Add(item);
                var _ = Task.Run(async () => await GetFiles());
            }
            else
            {
                throw new NotImplementedException();
            }
            OnPropertyChanged(nameof(CurrentFolderName));
        }
    }
}
