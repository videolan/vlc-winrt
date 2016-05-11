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
using libVLCX;

namespace VLC_WinRT.Model
{
    public class VLCStorageFolder : BindableBase, IVLCStorageItem
    {
        private StorageFolder storageItem;
        private Media media;

        private bool isLoading;
        private string name;
        private string lastModified;
        private string sizeHumanizedString = "";
        private int filesCount;

        public VLCStorageFolder(StorageFolder folder)
        {
            storageItem = folder;
        }

        public VLCStorageFolder(Media media)
        {
            this.media = media;
        }

        async Task Initialize()
        {
            if (storageItem != null)
            {
                var props = await storageItem.GetBasicPropertiesAsync();
                name = storageItem.Name;
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
                {
                    OnPropertyChanged(nameof(Name));
                    if (props.DateModified.Year == 1601) return;
                    lastModified = props.DateModified.ToString("dd/MM/yyyy hh:mm");
                    OnPropertyChanged(nameof(LastModified));
                });
            }
            else if (media != null)
            {
                name = media.meta(MediaMeta.Title);

                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
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
        public bool SizeAvailable => false;

        public IStorageItem StorageItem => storageItem;

        public Media Media => media;
    }
}
