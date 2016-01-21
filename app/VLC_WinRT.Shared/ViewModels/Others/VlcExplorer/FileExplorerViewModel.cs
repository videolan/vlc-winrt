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

namespace VLC_WinRT.ViewModels.Others.VlcExplorer
{
    public class FileExplorerViewModel : BindableBase
    {
        #region private props
        private bool _isFolderEmpty;
        private RootFolderType type;
        #endregion

        #region private fields
        private ObservableCollection<StorageFolder> _backStack = new ObservableCollection<StorageFolder>();
        private ObservableCollection<IVLCStorageItem> _storageItems = new ObservableCollection<IVLCStorageItem>();
        #endregion

        #region public props
        public string Id;
        public string Name { get; set; }

        public IStorageItemClickedCommand NavigateToCommand { get; } = new IStorageItemClickedCommand();

        public GoUpperFolderCommand GoBackCommand { get; } = new GoUpperFolderCommand();

        public PlayFolderCommand PlayFolderCommand { get; } = new PlayFolderCommand();

        public bool CanGoBack
        {
            get { return BackStack.Count > 1; }
        }

        public bool IsFolderEmpty
        {
            get { return _isFolderEmpty; }
            set { SetProperty(ref _isFolderEmpty, value); }
        }

        public string CurrentFolderName
        {
            get { return BackStack.Last().Name; }
        }

        public RootFolderType Type => type;

        #endregion

        #region public fields
        public ObservableCollection<IVLCStorageItem> StorageItems
        {
            get { return _storageItems; }
            set { SetProperty(ref _storageItems, value); }
        }

        public ObservableCollection<StorageFolder> BackStack
        {
            get { return _backStack; }
            set { SetProperty(ref _backStack, value); }
        }

        #endregion

        public FileExplorerViewModel(StorageFolder root, RootFolderType ftype, string id = null)
        {
            type = ftype;
            NavigateToCommand = new IStorageItemClickedCommand();
            GoBackCommand = new GoUpperFolderCommand();
            BackStack.Add(root);
            Name = root.DisplayName;
            if (id != null)
                Id = id;
        }

        public async Task GetFiles()
        {
            try
            {
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
                {
                    _storageItems.Clear();
                    IsFolderEmpty = false;
                });
                IReadOnlyList<IStorageItem> items = null;
#if WINDOWS_APP
                var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, VLCFileExtensions.Supported);
                var fileQuery = BackStack.Last().CreateItemQueryWithOptions(queryOptions);
                items = await fileQuery.GetItemsAsync();
#else
                items = await BackStack.Last().GetItemsAsync();
#endif
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
                    OnPropertyChanged("StorageItems");
                });
            }
            catch (Exception exception)
            {
                LogHelper.Log("Failed to index folders and files in " + BackStack.Last().DisplayName + "\n" + exception.ToString());
            }
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
            {
                OnPropertyChanged("CanGoBack");
                IsFolderEmpty = !StorageItems.Any();
            });
        }

        public async Task NavigateTo(IVLCStorageItem storageItem)
        {
            var item = storageItem as VLCStorageFolder;
            if (item != null)
            {
                BackStack.Add(item.StorageItem as StorageFolder);
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
            OnPropertyChanged("CurrentFolderName");
        }

        public void GoBack()
        {
            if (BackStack.Count > 1)
            {
                BackStack.Remove(BackStack.Last());
                Task.Run(() => GetFiles());
                OnPropertyChanged("CanGoBack");
                OnPropertyChanged("CurrentFolderName");
            }
        }
    }
}
