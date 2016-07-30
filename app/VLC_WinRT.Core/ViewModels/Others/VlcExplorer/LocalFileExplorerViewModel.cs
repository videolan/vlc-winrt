using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using VLC_WinRT.Helpers;
using VLC_WinRT.Model;
using Windows.Storage.Search;
using VLC_WinRT.Commands.VLCFileExplorer;
using VLC_WinRT.Model.FileExplorer;
using VLC_WinRT.Utils;
using Windows.UI.Xaml;
using libVLCX;

namespace VLC_WinRT.ViewModels.Others.VlcExplorer
{
    public class LocalFileExplorerViewModel : FileExplorer
    {
        public LocalFileExplorerViewModel(StorageFolder root, RootFolderType ftype, string id = null)
            : base(root.DisplayName, ftype)
        {
            BackStack.Add(new VLCStorageFolder(root));
            if (id != null)
                Id = id;
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
                IReadOnlyList<IStorageItem> items = null;
                var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, VLCFileExtensions.Supported);
                var fileQuery = (BackStack.Last().StorageItem as StorageFolder).CreateItemQueryWithOptions(queryOptions);
                items = await fileQuery.GetItemsAsync();
                var vlcItems = new ObservableCollection<IVLCStorageItem>();
                foreach (var storageItem in items)
                {
                    if (storageItem.IsOfType(StorageItemTypes.File))
                    {
                        var file = new VLCStorageFile(storageItem as StorageFile);
                        vlcItems.Add(file);
                    }
                    else if (storageItem.IsOfType(StorageItemTypes.Folder))
                    {
                        var folder = new VLCStorageFolder(storageItem as StorageFolder);
                        vlcItems.Add(folder);
                    }
                }
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
                {
                    StorageItems = new ObservableCollection<IVLCStorageItem>(vlcItems);
                    OnPropertyChanged(nameof(StorageItems));
                    IsFolderEmpty = !StorageItems.Any();
                    IsLoadingFiles = false;
                });
            }
            catch (Exception exception)
            {
                LogHelper.Log("Failed to index folders and files in " + BackStack.Last().Name + "\n" + exception.ToString());
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
                {
                    IsFolderEmpty = !StorageItems.Any();
                    IsLoadingFiles = false;
                });
            }
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
                var vlcFile = storageItem as VLCStorageFile;
                if (vlcFile == null) return;
                var file = vlcFile.StorageItem as StorageFile;
                if (VLCFileExtensions.AudioExtensions.Contains(file.FileType))
                {
                    await Locator.MediaPlaybackViewModel.PlayAudioFile(file);
                }
                else if (VLCFileExtensions.VideoExtensions.Contains(file.FileType))
                {
                    await Locator.MediaPlaybackViewModel.PlayVideoFile(file);
                }
            }
            OnPropertyChanged(nameof(CurrentFolderName));
        }
    }
}
