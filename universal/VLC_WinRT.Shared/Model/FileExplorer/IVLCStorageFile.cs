using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Core;
using VLC_WinRT.Model.FileExplorer;
using Windows.UI.Xaml.Media.Imaging;
using VLC_WinRT.Utils;
using WinRTXamlToolkit.IO.Extensions;

namespace VLC_WinRT.Model
{
    public class VLCStorageFile : BindableBase, IVLCStorageItem
    {
        private StorageFile storageItem;
        private bool isLoading;
        private string name;
        private string lastModified;
        private string sizeHumanizedString;


        public VLCStorageFile(StorageFile folder)
        {
            storageItem = folder;
            name = folder.Name;
        }

        async Task Initialize()
        {
            var props = await storageItem.GetBasicPropertiesAsync();
            var size = await storageItem.GetSize();
            var sizeString = "";
            if (size > 0)
                sizeString = size.GetSizeString();


            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                lastModified = props.DateModified.ToString("dd/MM/yyyy hh:mm");
                sizeHumanizedString = sizeString;
                OnPropertyChanged(nameof(LastModified));
                OnPropertyChanged(nameof(SizeHumanizedString));
                OnPropertyChanged(nameof(SizeAvailable));
            });
            //var storage = await storageItem.GetThumbnailAsync(ThumbnailMode.ListView);
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

        public bool SizeAvailable => (!string.IsNullOrEmpty(sizeHumanizedString));

        public IStorageItem StorageItem => storageItem;

        public StorageItemThumbnail Thumbnail
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
}
