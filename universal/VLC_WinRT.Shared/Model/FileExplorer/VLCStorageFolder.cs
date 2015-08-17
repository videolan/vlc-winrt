using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Core;
using SQLite;
using VLC_WinRT.Model.FileExplorer;
using Windows.UI.Xaml.Media.Imaging;
using VLC_WinRT.Utils;
using WinRTXamlToolkit.IO.Extensions;

namespace VLC_WinRT.Model
{
    public class VLCStorageFolder : BindableBase, IVLCStorageItem
    {
        private StorageFolder storageItem;
        private bool isLoading;
        private string name;
        private string lastModified;
        private string sizeHumanizedString;
        private int filesCount;

        public VLCStorageFolder(StorageFolder folder)
        {
            storageItem = folder;
            name = folder.DisplayName;
            sizeHumanizedString = "";
        }

        async Task Initialize()
        {
            var props = await storageItem.GetBasicPropertiesAsync();
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                if (props.DateModified.Year == 1601) return;
                lastModified = props.DateModified.ToString("dd/MM/yyyy hh:mm");
                OnPropertyChanged(nameof(LastModified));
            });
        }

        public string Name
        {
            get
            {
                if (!isLoading)
                {
                    isLoading = true;
                    Task.Run(() => Initialize());
                }
                return name;
            }
        }
        public string LastModified => lastModified;

        public string SizeHumanizedString => sizeHumanizedString;
        public bool SizeAvailable => false;

        public IStorageItem StorageItem => storageItem;

        public StorageItemThumbnail Thumbnail
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
}
