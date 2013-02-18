using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;
using Windows.Devices.Enumeration;
using Windows.Devices.Portable;
using Windows.Storage;
using Windows.Storage.Search;

namespace VLC_WINRT.Utility.Services
{
    public class MediaScanner : IDisposable
    {
        //TODO: provide a better way to describe to the app what file types are supported
        private static readonly List<string> ValidFiles = new List<string> {".m4v", ".mp4", ".mp3", ".avi"};

        // Watcher that will notify us when USB drives, SD Cards, etc are inserted
        private static readonly MediaScanner _instance = new MediaScanner();
        private DeviceWatcher _deviceWatcher;

        private MediaScanner()
        {
            _deviceWatcher = DeviceInformation.CreateWatcher(DeviceClass.PortableStorageDevice);
            _deviceWatcher.Added += DeviceAdded;
            _deviceWatcher.Removed += DeviceRemoved;
            _deviceWatcher.Start();
        }

        public static MediaScanner Instance
        {
            get { return _instance; }
        }

        public void Dispose()
        {
            if (_deviceWatcher != null)
            {
                _deviceWatcher.Stop();
                _deviceWatcher.Added -= DeviceAdded;
                _deviceWatcher.Removed -= DeviceRemoved;
                _deviceWatcher = null;
            }
        }

        public static async Task<IReadOnlyList<StorageFile>> GetMediaFromFolder(StorageFolder folder, uint numberOfFiles,
                                                                       CommonFileQuery query)
        {
            var queryOptions = new QueryOptions(CommonFileQuery.OrderByName, new List<string> { ".mkv", ".mp4", ".m4v", ".avi" });
            var imageFileQuery = folder.CreateFileQueryWithOptions(queryOptions);

            var files = await imageFileQuery.GetFilesAsync(0, numberOfFiles);
            return files;
        }

        private void DeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            Debug.WriteLine("Device Removed.");
        }

        private async void DeviceAdded(DeviceWatcher sender, DeviceInformation args)
        {
            var devices = await DeviceInformation.FindAllAsync(DeviceClass.PortableStorageDevice);
            var mainVM = ViewModelLocator.MainPageVM;

            foreach (DeviceInformation device in devices)
            {
                Debug.WriteLine("Device added: "+ device.Name);

                var folder = StorageDevice.FromId(device.Id);
                mainVM.RemovableStorageVMs.Add(new LibraryViewModel(folder));
            }
        }
    }
}