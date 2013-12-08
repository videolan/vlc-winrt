using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Portable;
using Windows.Storage;
using Windows.Storage.Search;
using VLC_WINRT.Common;
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;

namespace VLC_WINRT.Utility.Services.RunTime
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
            IReadOnlyList<StorageFile> files = null;
            StorageFileQueryResult fileQuery;
            var queryOptions = new QueryOptions(query,
                                               new List<string> { ".mkv", ".mp4", ".m4v", ".avi", ".mp3", ".wav" });
            try
            {
                fileQuery = folder.CreateFileQueryWithOptions(queryOptions);

                files = await fileQuery.GetFilesAsync(0, numberOfFiles);
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine("exception listing files");
                Debug.WriteLine(ex.ToString());
            }

            // DLNA folders don't support advanced file listings, us a basic file query
            if (files == null)
            {
                fileQuery = folder.CreateFileQuery(CommonFileQuery.OrderByName);
                files = await fileQuery.GetFilesAsync();
            }

            return files;
        }

        private async void DeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            Debug.WriteLine("Device Removed.");
            DeviceInformationCollection devices =
                await DeviceInformation.FindAllAsync(DeviceClass.PortableStorageDevice);
            MainPageViewModel mainVM = Locator.MainPageVM;


            foreach (LibraryViewModel removableStorageVM in mainVM.RemovableStorageVMs)
            {
                bool found = false;
                foreach (DeviceInformation device in devices)
                {
                    StorageFolder folder = StorageDevice.FromId(device.Id);
                    if (removableStorageVM.Location == folder)
                        found = true;
                }

                if (!found)
                {
                    LibraryViewModel vm = removableStorageVM;
                    DispatchHelper.Invoke(() => mainVM.RemovableStorageVMs.Remove(vm));
                }
            }
        }

        private async void DeviceAdded(DeviceWatcher sender, DeviceInformation args)
        {
            DeviceInformationCollection devices =
                await DeviceInformation.FindAllAsync(DeviceClass.PortableStorageDevice);
            MainPageViewModel mainVM = Locator.MainPageVM;

            foreach (DeviceInformation device in devices)
            {
                Debug.WriteLine("Device added: " + device.Name);

                StorageFolder folder = StorageDevice.FromId(device.Id);
                DispatchHelper.Invoke(() => mainVM.RemovableStorageVMs.Add(new LibraryViewModel(folder)));
            }
        }
    }
}