/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Storage;
using VLC_WinRT.Commands.VLCFileExplorer;
using VLC_WinRT.Services.RunTime;
using VLC_WinRT.ViewModels.Others.VlcExplorer;
using System.Threading.Tasks;
using Autofac;
using VLC_WinRT.Utils;
using VLC_WinRT.Helpers;
using VLC_WinRT.Model.FileExplorer;
using Windows.UI.Core;

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
        private ObservableCollection<FileExplorerViewModel> _storageVMs;
        #endregion

        #region public props

        public RemovableDeviceClickedCommand RemovableDeviceClicked { get; } = new RemovableDeviceClickedCommand();

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

        public IEnumerable<IGrouping<RootFolderType, FileExplorerViewModel>> StorageVMsGrouped
        {
            get { return _storageVMs?.GroupBy(x => x.Type); }
        } 
        #endregion
        public void OnNavigatedTo()
        {
            _storageVMs = new ObservableCollection<FileExplorerViewModel>();
            var musicLibrary = new FileExplorerViewModel(KnownFolders.MusicLibrary, RootFolderType.Library);
            StorageVMs.Add(musicLibrary);
            var videoLibrary = new FileExplorerViewModel(KnownFolders.VideosLibrary, RootFolderType.Library);
            StorageVMs.Add(videoLibrary);
            CurrentStorageVM = StorageVMs[0];
            Task.Run(() => CurrentStorageVM?.GetFiles());
#if WINDOWS_APP
            Task.Run(() => InitializeDLNA());
            _deviceService = App.Container.Resolve<ExternalDeviceService>();
            _deviceService.ExternalDeviceAdded += DeviceAdded;
            _deviceService.ExternalDeviceRemoved += DeviceRemoved;
#else
            Task.Run(() => InitializeSDCard());
#endif
            OnPropertyChanged(nameof(StorageVMsGrouped));
        }

        public void Dispose()
        {
#if WINDOWS_PHONE_APP
#elif WINDOWS_APP
            _deviceService.ExternalDeviceAdded -= DeviceAdded;
            _deviceService.ExternalDeviceRemoved -= DeviceRemoved;
            _deviceService.Dispose();
            _deviceService = null;
#endif
            _currentStorageVM = null;
            _storageVMs?.Clear();
            _storageVMs = null;
            GC.Collect();
        }

        private async Task InitializeSDCard()
        {
            var devices = KnownFolders.RemovableDevices;
            var cards = await devices.GetFoldersAsync();
            if (cards.Any())
            {
                var external = new FileExplorerViewModel(cards[0], RootFolderType.ExternalDevice);
                await DispatchHelper.InvokeAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    StorageVMs.Add(external);
                    OnPropertyChanged(nameof(StorageVMsGrouped));
                });
            }
        }

        private async void InitializeDLNA()
        {
            try
            {
                var dlnaFolders = await KnownFolders.MediaServerDevices.GetFoldersAsync();
                foreach (var dlnaFolder in dlnaFolders)
                {
                    var folder = new FileExplorerViewModel(dlnaFolder, RootFolderType.Network);
                    await DispatchHelper.InvokeAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        StorageVMs.Add(folder);
                        OnPropertyChanged(nameof(StorageVMsGrouped));
                    });
                }
            }
            catch
            {
                LogHelper.Log("Failed to Get MediaServerDevices");
            }
        }

#if WINDOWS_APP
        private async void DeviceAdded(object sender, string id)
        {
            await AddFolder(id);
        }

        private async Task AddFolder(string newId)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    if (StorageVMs.All(vm => vm.Id != newId))
                    {
                        var external = new FileExplorerViewModel(StorageDevice.FromId(newId), RootFolderType.ExternalDevice, newId);
                        StorageVMs.Add(external);
                    }
                    if (StorageVMs.Any())
                    {
                        CurrentStorageVM = StorageVMs[0];
                    }
                    OnPropertyChanged(nameof(StorageVMsGrouped));
                }
                catch { }
            });
        }

        private async void DeviceRemoved(object sender, string id)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
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
                OnPropertyChanged(nameof(StorageVMsGrouped));
            });
        }
#endif
    }
}
