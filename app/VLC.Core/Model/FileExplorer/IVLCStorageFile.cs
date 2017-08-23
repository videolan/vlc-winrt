using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Core;
using VLC.Model.FileExplorer;
using Windows.UI.Xaml.Media.Imaging;
using VLC.Utils;
using WinRTXamlToolkit.IO.Extensions;
using libVLCX;

namespace VLC.Model
{
    public class VLCStorageFile : BindableBase, IVLCStorageItem
    {
        private StorageFile storageItem;
        private Media media;

        private bool isLoading;
        private string name;
        private string lastModified;
        private string sizeHumanizedString;


        public VLCStorageFile(StorageFile folder)
        {
            storageItem = folder;
        }

        public VLCStorageFile(Media media)
        {
            this.media = media;
        }

        async Task Initialize()
        {
            if (storageItem != null)
            {
                var props = await storageItem.GetBasicPropertiesAsync();
                //var size = await storageItem.GetSizeAsync();

                var sizeString = "";
                //if (size > 0)
                //    sizeString = size.GetSizeString();

                name = storageItem.DisplayName;
                await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Low, () =>
                {
                    lastModified = props.DateModified.ToString("dd/MM/yyyy hh:mm");
                    sizeHumanizedString = sizeString;
                    OnPropertyChanged(nameof(LastModified));
                    OnPropertyChanged(nameof(SizeHumanizedString));
                    OnPropertyChanged(nameof(SizeAvailable));
                    OnPropertyChanged(nameof(Name));
                });
            }
            else if (media != null)
            {
                this.name = media.meta(MediaMeta.Title);

                await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Low, () =>
                {
                    OnPropertyChanged(nameof(Name));
                });
            }
        }

        public string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(name))
                    return name;
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

        public Media Media => media;
    }
}
