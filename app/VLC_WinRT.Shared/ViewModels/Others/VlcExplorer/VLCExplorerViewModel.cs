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
using Windows.Devices.Portable;
using Windows.UI.Xaml;
using System.Diagnostics;

#if WINDOWS_PHONE_APP
#else
#endif

namespace VLC_WinRT.ViewModels.RemovableDevicesVM
{
    public class VLCExplorerViewModel : BindableBase, IDisposable
    {
        #region private props
        private ExternalDeviceService _deviceService;
        private FileExplorer _currentStorageVM;
        #endregion

        #region private fields
        private ObservableCollection<GroupItemList<FileExplorer>> _fileExplorersGrouped = new ObservableCollection<GroupItemList<FileExplorer>>();
        #endregion

        #region public props
        private Visibility _rootFoldersVisibility;
        private Visibility _fileExplorerVisibility;

        public RootFolderClickedCommand RootFolderClicked { get; } = new RootFolderClickedCommand();
        public GoUpperFolderCommand GoBackCommand { get; } = new GoUpperFolderCommand();

        public bool CanGoBack
        {
            get { return CurrentStorageVM?.BackStack.Count > 1; }
        }

        public FileExplorer CurrentStorageVM
        {
            get
            {
                return _currentStorageVM;
            }
            set
            {
                SetProperty(ref _currentStorageVM, value);
                if (value != null)
                {
                    RootFoldersVisibility = Visibility.Collapsed;
                    FileExplorerVisibility = Visibility.Visible;
                }
                else
                {
                    FileExplorerVisibility = Visibility.Collapsed;
                    RootFoldersVisibility = Visibility.Visible;
                }
            }
        }
        #endregion

        #region public fields
        public ObservableCollection<GroupItemList<FileExplorer>> FileExplorersGrouped
        {
            get { return _fileExplorersGrouped; }
            set { SetProperty(ref _fileExplorersGrouped, value); }
        }

        public Visibility RootFoldersVisibility
        {
            get { return _rootFoldersVisibility; }
            set { SetProperty(ref _rootFoldersVisibility, value); }
        }


        public Visibility FileExplorerVisibility
        {
            get { return _fileExplorerVisibility; }
            set { SetProperty(ref _fileExplorerVisibility, value); }
        }
        #endregion
        public async Task OnNavigatedTo()
        {
            var musicFolder = new LocalFileExplorerViewModel(KnownFolders.MusicLibrary, RootFolderType.Library);
            musicFolder.LogoGlyph = App.Current.Resources["MusicFilledSymbol"] as string;
            await AddFolder(musicFolder);
            var videoFolder = new LocalFileExplorerViewModel(KnownFolders.VideosLibrary, RootFolderType.Library);
            videoFolder.LogoGlyph = App.Current.Resources["VideoFilledSymbol"] as string;
            await AddFolder(videoFolder);
            var picFolder = new LocalFileExplorerViewModel(KnownFolders.PicturesLibrary, RootFolderType.Library);
            picFolder.LogoGlyph = App.Current.Resources["BuddySymbol"] as string;
            await AddFolder(picFolder);

#if WINDOWS_PHONE_APP
            Task.Run(() => InitializeSDCard());
#else
            _deviceService = App.Container.Resolve<ExternalDeviceService>();
            _deviceService.ExternalDeviceAdded += DeviceAdded;
            _deviceService.ExternalDeviceRemoved += DeviceRemoved;
#endif
            FileExplorerVisibility = Visibility.Collapsed;
            RootFoldersVisibility = Visibility.Visible;
            await Task.Run(async () =>
            {
                Locator.VLCService.MediaListItemAdded += VLCService_MediaListItemAdded;
                Locator.VLCService.MediaListItemDeleted += VLCService_MediaListItemDeleted;
                await Locator.VLCService.InitDiscoverer();
            });
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
            Locator.VLCService.MediaListItemAdded -= VLCService_MediaListItemAdded;
            Locator.VLCService.MediaListItemDeleted -= VLCService_MediaListItemDeleted;
            Locator.VLCService.DisposeDiscoverer();
            _currentStorageVM = null;
            _fileExplorersGrouped?.Clear();
            GC.Collect();
        }

        private async Task InitializeSDCard()
        {
            var devices = KnownFolders.RemovableDevices;
            var cards = await devices.GetFoldersAsync();
            if (cards.Any())
            {
                var external = new LocalFileExplorerViewModel(cards[0], RootFolderType.ExternalDevice);
                await AddFolder(external);
            }
        }

#if WINDOWS_PHONE_APP
#else
        private async void DeviceAdded(object sender, string id)
        {
            await AddFolder(id);
        }

        private async Task AddFolder(string newId)
        {
            if (DeviceTypeHelper.GetDeviceType() != DeviceTypeEnum.Tablet) return;
            try
            {
                var external = new LocalFileExplorerViewModel(StorageDevice.FromId(newId), RootFolderType.ExternalDevice, newId);
                await AddFolder(external);
            }
            catch { }
        }

        private async void DeviceRemoved(object sender, string id)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                LocalFileExplorerViewModel removedViewModel = FileExplorersGrouped.FirstOrDefault(x => (RootFolderType)x.Key == RootFolderType.ExternalDevice)?.FirstOrDefault(vm => vm.Id == id) as LocalFileExplorerViewModel;
                if (removedViewModel != null)
                {
                    if (CurrentStorageVM == removedViewModel)
                    {
                        CurrentStorageVM.StorageItems.Clear();
                        CurrentStorageVM = null;
                    }
                    FileExplorersGrouped.FirstOrDefault(x => x.Contains(removedViewModel)).Remove(removedViewModel);
                    GC.Collect();
                }
            });
        }
#endif

        private async void VLCService_MediaListItemAdded(libVLCX.Media media, int index)
        {
            try
            {
                var localNetwork = new VLCFileExplorerViewModel(media, RootFolderType.Network);
                await AddFolder(localNetwork);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        private async void VLCService_MediaListItemDeleted(libVLCX.Media media, int index)
        {
            var fileExType = FileExplorersGrouped.FirstOrDefault(x => (RootFolderType)x.Key == RootFolderType.Network);
            if (fileExType == null)
                return;
            var fileEx = fileExType.FirstOrDefault(x => x.Name == media.meta(libVLCX.MediaMeta.Title));
            if (fileEx == null)
                return;
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () => fileExType.Remove(fileEx));
        }

        async Task AddFolder(FileExplorer fileEx)
        {
            var key = FileExplorersGrouped.FirstOrDefault(x => (RootFolderType)x.Key == fileEx.Type);
            if (key == null)
            {
                key = new GroupItemList<FileExplorer>(fileEx) { Key = fileEx.Type };
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () => FileExplorersGrouped.Add(key));
            }
            else await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () => key.Add(fileEx));
        }

        public void GoBackToRootFolders()
        {
            CurrentStorageVM = null;
        }
    }
}
