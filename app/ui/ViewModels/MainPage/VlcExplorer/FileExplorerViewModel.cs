using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using VLC_WINRT.Common;

namespace VLC_WINRT.ViewModels.MainPage.VlcExplorer
{
    public interface IVlcStorageItem
    {
    }

    public class VlcStorageFile : IVlcStorageItem
    {
        public StorageFile StorageFile { get; set; }

        public string Name
        {
            get;
            set;
        }

        public string Path
        {
            get;
            set;
        }
    }

    public class FileExplorerViewModel : IVlcStorageItem
    {
        private StorageFolder _rootFolder;
        private ObservableCollection<IVlcStorageItem> _storageItems = new ObservableCollection<IVlcStorageItem>();
        private string _name;
        public string Id;

        public ObservableCollection<IVlcStorageItem> StorageItems
        {
            get { return _storageItems; }
            set { _storageItems = value; }
        }

        public string Name { get { return _name; } set { _name = value; } }

        public FileExplorerViewModel(StorageFolder root, string id)
        {
            _rootFolder = root;
            Id = id;
            _name = root.DisplayName;
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

                foreach (var storageItem in items)
                {
                    IVlcStorageItem vlcStorageItem = null;
                    if (storageItem.IsOfType(StorageItemTypes.File)) 
                    {
                        vlcStorageItem = new VlcStorageFile();
                        ((VlcStorageFile)vlcStorageItem).Name = storageItem.Name;
                        ((VlcStorageFile)vlcStorageItem).Path = storageItem.Path;
                        ((VlcStorageFile)vlcStorageItem).StorageFile = (StorageFile)storageItem;
                    }
                    else if (storageItem.IsOfType(StorageItemTypes.Folder))
                    {
                        vlcStorageItem = new FileExplorerViewModel((StorageFolder)storageItem,
                            ((StorageFolder)storageItem).FolderRelativeId);
                    }

                    if (vlcStorageItem != null)
                    {
                        _storageItems.Add(vlcStorageItem);
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Failed to index folders and files in " + _rootFolder.DisplayName + "\n" + exception.ToString());
            }
        }
    }
}
