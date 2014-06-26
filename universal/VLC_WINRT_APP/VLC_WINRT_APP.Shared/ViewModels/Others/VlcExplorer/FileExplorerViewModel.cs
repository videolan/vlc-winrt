using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Core;
using Windows.UI.Popups;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.Model;

namespace VLC_WINRT_APP.ViewModels.Others.VlcExplorer
{
    public class FileExplorerViewModel : BindableBase
    {
        private StorageFolder _rootFolder;
        private ObservableCollection<IStorageItem> _storageItems = new ObservableCollection<IStorageItem>();
        public string Id;

        public ObservableCollection<IStorageItem> StorageItems
        {
            get { return _storageItems; }
            set { SetProperty(ref _storageItems, value); }
        }

        public string Name { get; set; }

        public FileExplorerViewModel(StorageFolder root, string id = null)
        {
            _rootFolder = root;
            Name = root.DisplayName;

            if (id != null)
                Id = id;
        }

        public async Task GetFiles()
        {
            try
            {
                App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _storageItems.Clear());
                var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, VLCFileExtensions.Supported);

                var fileQuery = _rootFolder.CreateItemQueryWithOptions(queryOptions);
                var items = await fileQuery.GetItemsAsync();
                foreach (IStorageItem storageItem in items)
                {
                    App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                    {
                        StorageItems.Add(storageItem);
                        OnPropertyChanged("StorageItems");
                    });
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Failed to index folders and files in " + _rootFolder.DisplayName + "\n" + exception.ToString());
            }
        }
    }
}
