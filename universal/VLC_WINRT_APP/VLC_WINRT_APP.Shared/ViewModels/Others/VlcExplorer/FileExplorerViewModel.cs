using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Popups;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Common;

namespace VLC_WINRT_APP.ViewModels.Others.VlcExplorer
{
    public class Storage
    {
        public string Path { get; set; }
    }
    public class FileExplorerViewModel : BindableBase
    {
        private StorageFolder _rootFolder;
        private ObservableCollection<Storage> _storageItems = new ObservableCollection<Storage>();
        private string _name;
        public string Id;

        public ObservableCollection<Storage> StorageItems
        {
            get { return _storageItems; }
            set { SetProperty(ref _storageItems, value); }
        }
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public FileExplorerViewModel(StorageFolder root, string id = null)
        {
            _rootFolder = root;
            _name = root.DisplayName;

            if (id != null) Id = id;
        }

        public async Task GetFiles()
        {
            try
            {
                _storageItems.Clear();
                var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery,
                                                   new List<string>
                                               {
                                                   ".3g2",
                                                   ".3gp",
                                                   ".3gp2",
                                                   ".3gpp",
                                                   ".amv",
                                                   ".asf",
                                                   ".avi",
                                                   ".divx",
                                                   ".drc",
                                                   ".dv",
                                                   ".f4v",
                                                   ".flv",
                                                   ".gvi",
                                                   ".gxf",
                                                   ".ismv",
                                                   ".iso",
                                                   ".m1v",
                                                   ".m2v",
                                                   ".m2t",
                                                   ".m2ts",
                                                   ".m3u8",
                                                   ".mkv",
                                                   ".mov",
                                                   ".mp2",
                                                   ".mp2v",
                                                   ".mp4",
                                                   ".mp4v",
                                                   ".mpe",
                                                   ".mpeg",
                                                   ".mpeg1",
                                                   ".mpeg2",
                                                   ".mpeg4",
                                                   ".mpg",
                                                   ".mpv2",
                                                   ".mts",
                                                   ".mtv",
                                                   ".mxf",
                                                   ".mxg",
                                                   ".nsv",
                                                   ".nut",
                                                   ".nuv",
                                                   ".ogm",
                                                   ".ogv",
                                                   ".ogx",
                                                   ".ps",
                                                   ".rec",
                                                   ".rm",
                                                   ".rmvb",
                                                   ".tob",
                                                   ".ts",
                                                   ".tts",
                                                   ".vob",
                                                   ".vro",
                                                   ".webm",
                                                   ".wm",
                                                   ".wmv",
                                                   ".wtv",
                                                   ".xesc",
                                                   ".mp3",
                                                   ".ogg",
                                                   ".aac",
                                                   ".wma",
                                                   ".wav",
                                                   ".flac",
                                               });

                var fileQuery = _rootFolder.CreateItemQueryWithOptions(queryOptions);
                var items = await fileQuery.GetItemsAsync();
                foreach (IStorageItem storageItem in items)
                {
                    StorageItems.Add(new Storage() { Path = storageItem.Path });
                    OnPropertyChanged("StorageItems");
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Failed to index folders and files in " + _rootFolder.DisplayName + "\n" + exception.ToString());
            }
        }
    }
}
