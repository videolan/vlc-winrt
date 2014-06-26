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
using Windows.Devices.Portable;
using Autofac;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.Services.RunTime;
using VLC_WINRT_APP.ViewModels.Others.VlcExplorer;

namespace VLC_WINRT_APP.ViewModels.RemovableDevicesVM
{
#if WINDOWS_APP
    public class ExternalStorageViewModel : BindableBase, IDisposable
    {
        private ExternalDeviceService _deviceService;

        private ObservableCollection<FileExplorerViewModel> _removableStorageVMs =
            new ObservableCollection<FileExplorerViewModel>();

        private FileExplorerViewModel _currentStorageVM;

        public ExternalStorageViewModel()
        {
            _deviceService = App.Container.Resolve<ExternalDeviceService>();
            _deviceService.ExternalDeviceAdded += DeviceAdded;
            _deviceService.ExternalDeviceRemoved += DeviceRemoved;
        }

        public ObservableCollection<FileExplorerViewModel> RemovableStorageVMs
        {
            get { return _removableStorageVMs; }
            set { SetProperty(ref _removableStorageVMs, value); }
        }

        public FileExplorerViewModel CurrentStorageVM
        {
            get
            {
                if (RemovableStorageVMs.Any())
                {
                    if(!RemovableStorageVMs[0].StorageItems.Any())
                        Task.Run(() => RemovableStorageVMs[0].GetFiles());
                    return RemovableStorageVMs[0];
                }
                return _currentStorageVM;
            }
            set { SetProperty(ref _currentStorageVM, value); }
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
                    RemovableStorageVMs.Add(external);
                }
                if (RemovableStorageVMs.Count == 1)
                {
                    CurrentStorageVM = RemovableStorageVMs[0];
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
                    if (CurrentStorageVM == removedViewModel)
                    {
                        CurrentStorageVM.StorageItems.Clear();
                        CurrentStorageVM = null;
                    }
                    RemovableStorageVMs.Remove(removedViewModel);
                    GC.Collect();
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
