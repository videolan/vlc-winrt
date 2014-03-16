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
using Windows.Devices.Portable;
using Windows.System.Threading;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.IoC;
using VLC_WINRT.Utility.Services.RunTime;
using System.Threading.Tasks;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class ExternalStorageViewModel : BindableBase, IDisposable
    {
        private ExternalDeviceService _deviceService;

        private ObservableCollection<RemovableLibraryViewModel> _removableStorageVMs =
            new ObservableCollection<RemovableLibraryViewModel>();

        public ExternalStorageViewModel()
        {
            _deviceService = IoC.GetInstance<ExternalDeviceService>();
            _deviceService.ExternalDeviceAdded += DeviceAdded;
            _deviceService.ExternalDeviceRemoved += DeviceRemoved;
        }

        public async Task Initialize()
        {
            var devices = await _deviceService.GetExternalDeviceIds();
            foreach (string id in devices)
            {
                string newId = id;
                await AddFolder(newId);
            }
        }

        public ObservableCollection<RemovableLibraryViewModel> RemovableStorageVMs
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
                    RemovableStorageVMs.Add(new RemovableLibraryViewModel(StorageDevice.FromId(newId), newId));
                }
            });
        }

        private async void DeviceRemoved(object sender, string id)
        {
            await DispatchHelper.InvokeAsync(() =>
            {
                RemovableLibraryViewModel removedViewModel = RemovableStorageVMs.FirstOrDefault(vm => vm.Id == id);
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
}
