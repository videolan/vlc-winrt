/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Autofac;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Services.RunTime;
using VLC_WINRT.ViewModels.MainPage.VlcExplorer;
using VLC_WINRT_APP;
#if NETFX_CORE
using Windows.Devices.Portable;
#endif

namespace VLC_WINRT.ViewModels.MainPage
{
#if NETFX_CORE
    public class ExternalStorageViewModel : BindableBase, IDisposable
    {
        private ExternalDeviceService _deviceService;

        private ObservableCollection<FileExplorerViewModel> _removableStorageVMs =
            new ObservableCollection<FileExplorerViewModel>();

        public ExternalStorageViewModel()
        {
            _deviceService = App.Container.Resolve<ExternalDeviceService>();
            _deviceService.ExternalDeviceAdded += DeviceAdded;
            _deviceService.ExternalDeviceRemoved += DeviceRemoved;
        }

        public async Task Initialize()
        {
            var devices = await KnownFolders.RemovableDevices.GetFoldersAsync();
            foreach (StorageFolder storageFolder in devices)
            {
                await AddFolder(storageFolder.FolderRelativeId);
            }

            //var devices1 = await _deviceService.GetExternalDeviceIds();
            //foreach (string id in devices1)
            //{
            //    string newId = id;
            //    await AddFolder(newId);
            //}
        }

        public ObservableCollection<FileExplorerViewModel> RemovableStorageVMs
        {
            get { return _removableStorageVMs; }
            set { SetProperty(ref _removableStorageVMs, value); }
        }

        public void Dispose()
        {
            _deviceService.ExternalDeviceAdded -= DeviceAdded;
            _deviceService.ExternalDeviceRemoved -= DeviceRemoved;

            _deviceService = null;
        }

        private async Task AddFolder(string newId)
        {
            await DispatchHelper.InvokeAsync(() =>
            {
                if (RemovableStorageVMs.All(vm => vm.Id != newId))
                {
                    var external = new FileExplorerViewModel(StorageDevice.FromId(newId), newId);
                    external.GetFiles();
                    RemovableStorageVMs.Add(external);
                }
            });
        }

        private async void DeviceRemoved(object sender, string id)
        {
            await DispatchHelper.InvokeAsync(() =>
            {
                FileExplorerViewModel removedViewModel = RemovableStorageVMs.FirstOrDefault(vm => vm.Id == id);
                if (removedViewModel != null)
                {
                    RemovableStorageVMs.Remove(removedViewModel);
                }
            });
        }

        private async void DeviceAdded(object sender, string id)
        {
            await AddFolder(id);
        }
    }
#endif
}
