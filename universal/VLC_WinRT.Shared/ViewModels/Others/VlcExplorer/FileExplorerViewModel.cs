using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage
/* Unmerged change from project 'VLC_WinRT.Windows'
Before:
using Windows.Storage.AccessCache;
using Windows.Storage.Search;
After:
using Windows.Storage.Search;
*/
;
using Windows.UI.Core;
using VLC_WinRT.Commands.RemovableDevices;
using VLC_WinRT.Common;
using VLC_WinRT.Helpers;

/* Unmerged change from project 'VLC_WinRT.Windows'
Before:
using VLC_WinRT.Helpers.MusicLibrary;
using VLC_WinRT.Model;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Services.RunTime;
using VLC_WinRT.Views.VideoPages;
After:
using VLC_WinRT.Model;
*/
using VLC_WinRT.Model;
using Windows.Storage.Search;
using VLC_WinRT.Commands.VLCFileExplorer;

namespace VLC_WinRT.ViewModels.Others.VlcExplorer
{
    public class FileExplorerViewModel : BindableBase
    {
        #region private props
        private bool _isFolderEmpty;
        #endregion

        #region private fields
        private ObservableCollection<StorageFolder> _backStack = new ObservableCollection<StorageFolder>();
        private ObservableCollection<IStorageItem> _storageItems = new ObservableCollection<IStorageItem>();
        #endregion

        #region public props
        public string Id;
        public string Name { get; set; }

        public IStorageItemClickedCommand NavigateToCommand { get; }=new IStorageItemClickedCommand();

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
        #endregion

        #region public fields
        public ObservableCollection<IStorageItem> StorageItems
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

        public FileExplorerViewModel(StorageFolder root, string id = null)
        {
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
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
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
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    StorageItems = new ObservableCollection<IStorageItem>(items);
                    OnPropertyChanged("StorageItems");
                });
            }
            catch (Exception exception)
            {
                LogHelper.Log("Failed to index folders and files in " + BackStack.Last().DisplayName + "\n" + exception.ToString());
            }
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                OnPropertyChanged("CanGoBack");
                IsFolderEmpty = !StorageItems.Any();
            });
        }

        public async Task NavigateTo(IStorageItem storageItem)
        {
            var item = storageItem as StorageFolder;
            if (item != null)
            {
                BackStack.Add(item);
                var _ = Task.Run(async () => await GetFiles());
            }
            else
            {
                var file = storageItem as StorageFile;
                if (file == null) return;
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
