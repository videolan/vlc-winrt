using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Core;
using SQLite;
using VLC.Model.FileExplorer;
using Windows.UI.Xaml.Media.Imaging;
using VLC.Utils;
using WinRTXamlToolkit.IO.Extensions;
using libVLCX;

namespace VLC.Model
{
    public class VLCStorageFolder : BindableBase, IVLCStorageItem
    {
        private StorageFolder storageItem;
        private Media media;

        private bool isLoading;
        private string name;
        private string lastModified;
        private string sizeHumanizedString = "";

        public VLCStorageFolder(StorageFolder folder)
        {
            storageItem = folder;
            name = storageItem.Name;
        }

        public VLCStorageFolder(Media media)
        {
            this.media = media;
            name = media.meta(MediaMeta.Title);
        }

        async Task Initialize()
        {
            if (storageItem != null)
            {
                var props = await storageItem.GetBasicPropertiesAsync();
                await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Low, () =>
                {
                    OnPropertyChanged(nameof(Name));
                    if (props.DateModified.Year == 1601) return;
                    lastModified = props.DateModified.ToString("dd/MM/yyyy hh:mm");
                    OnPropertyChanged(nameof(LastModified));
                });
            }
            else if (media != null)
            {
            }
        }

        public string Name => name;

        public string LastModified
        {
            get
            {
                if (string.IsNullOrEmpty(lastModified) && !isLoading)
                {
                    isLoading = true;
                    Task.Run(() => Initialize());
                }
                return lastModified;
            }
        }

        public string SizeHumanizedString => sizeHumanizedString;
        public bool SizeAvailable => false;

        public IStorageItem StorageItem => storageItem;

        public Media Media => media;
    }
}
