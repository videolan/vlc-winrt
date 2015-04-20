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
using Windows.Storage;
using VLC_WinRT.Commands.RemovableDevices;
using VLC_WinRT.Services.RunTime;
using VLC_WinRT.ViewModels.Others.VlcExplorer;
using System.Threading.Tasks;
using Autofac;
using VLC_WinRT.Utils;
#if WINDOWS_APP
using Windows.Devices.Portable;
#endif

namespace VLC_WinRT.ViewModels.RemovableDevicesVM
{
    public class VLCExplorerViewModel : BindableBase, IDisposable
    {
        #region private props
        private ExternalDeviceService _deviceService;
        private FileExplorerViewModel _currentStorageVM;
        #endregion

        #region private fields
        private ObservableCollection<FileExplorerViewModel> _storageVMs = new ObservableCollection<FileExplorerViewModel>();
        #endregion

        #region public props

        public RemovableDeviceClickedCommand RemovableDeviceClicked { get; } =new RemovableDeviceClickedCommand();

        public FileExplorerViewModel CurrentStorageVM
        {
            get
            {
                return _currentStorageVM;
            }
            set { SetProperty(ref _currentStorageVM, value); }
        }
        #endregion

        #region public fields
        public ObservableCollection<FileExplorerViewModel> StorageVMs
        {
            get { return _storageVMs; }
            set { SetProperty(ref _storageVMs, value); }
        }
        #endregion

        public VLCExplorerViewModel()
        {
            RemovableDeviceClicked = new RemovableDeviceClickedCommand();
            var musicLibrary = new FileExplorerViewModel(KnownFolders.MusicLibrary);
            StorageVMs.Add(musicLibrary);
            var videoLibrary = new FileExplorerViewModel(KnownFolders.VideosLibrary);
            StorageVMs.Add(videoLibrary);
            CurrentStorageVM = StorageVMs[0];
            CurrentStorageVM?.GetFiles();
#if WINDOWS_APP
            _deviceService = App.Container.Resolve<ExternalDeviceService>();
            _deviceService.ExternalDeviceAdded += DeviceAdded;
            _deviceService.ExternalDeviceRemoved += DeviceRemoved;
#else
            Initialize();           
#endif
        }

        private async void Initialize()
        {
            var devices = KnownFolders.RemovableDevices;
            var cards = await devices.GetFoldersAsync();
            if (cards.Any())
            {
                var external = new FileExplorerViewModel(cards[0]);
                StorageVMs.Add(external);
            }
        }

#if WINDOWS_APP
        private async void DeviceAdded(object sender, string id)
        {
            await AddFolder(id);
        }

        private async Task AddFolder(string newId)
        {
            await DispatchHelper.InvokeAsync(() =>
            {
                if (StorageVMs.All(vm => vm.Id != newId))
                {
                    var external = new FileExplorerViewModel(StorageDevice.FromId(newId), newId);
                    StorageVMs.Add(external);
                }
                if (StorageVMs.Any())
                {
                    CurrentStorageVM = StorageVMs[0];
                }
            });
        }

        private async void DeviceRemoved(object sender, string id)
        {
            await DispatchHelper.InvokeAsync(() =>
            {
                FileExplorerViewModel removedViewModel = StorageVMs.FirstOrDefault(vm => vm.Id == id);
                if (removedViewModel != null)
                {
                    if (CurrentStorageVM == removedViewModel)
                    {
                        CurrentStorageVM.StorageItems.Clear();
                        CurrentStorageVM = null;
                    }
                    StorageVMs.Remove(removedViewModel);
                    GC.Collect();
                }
            });
        }
#endif

        public void Dispose()
        {
#if WINDOWS_APP
            _deviceService.ExternalDeviceAdded -= DeviceAdded;
            _deviceService.ExternalDeviceRemoved -= DeviceRemoved;
            _deviceService = null;
#endif
        }
    }
}
