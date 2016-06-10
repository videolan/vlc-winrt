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
            if (FileExplorersGrouped.Any() == false)
            {
                var categories = Enum.GetValues(typeof(RootFolderType)).Cast<RootFolderType>();
                foreach ( var c in categories )
                    await CreateFolderCategory(c);
                var musicFolder = new LocalFileExplorerViewModel(KnownFolders.MusicLibrary, RootFolderType.Library);
                musicFolder.LogoGlyph = App.Current.Resources["MusicFilledSymbol"] as string;
                await AddToFolder(musicFolder);
                var videoFolder = new LocalFileExplorerViewModel(KnownFolders.VideosLibrary, RootFolderType.Library);
                videoFolder.LogoGlyph = App.Current.Resources["VideoFilledSymbol"] as string;
                await AddToFolder(videoFolder);
#if STARTS
#else
                var picFolder = new LocalFileExplorerViewModel(KnownFolders.PicturesLibrary, RootFolderType.Library);
                picFolder.LogoGlyph = App.Current.Resources["BuddySymbol"] as string;
                await AddFolder(picFolder);
#endif

#if WINDOWS_PHONE_APP
                await InitializeSDCard();
#else
            }
            _deviceService = App.Container.Resolve<ExternalDeviceService>();
            _deviceService.ExternalDeviceAdded += DeviceAdded;
            _deviceService.ExternalDeviceRemoved += DeviceRemoved;
#endif
            FileExplorerVisibility = Visibility.Collapsed;
            RootFoldersVisibility = Visibility.Visible;
            await Task.Run(async () =>
            {
                Locator.MediaLibrary.MediaListItemAdded += VLCService_MediaListItemAdded;
                Locator.MediaLibrary.MediaListItemDeleted += VLCService_MediaListItemDeleted;
                await Locator.MediaLibrary.InitDiscoverer();
            });
        }


        public async void Dispose()
        {
#if WINDOWS_PHONE_APP
#elif WINDOWS_APP
            _deviceService.ExternalDeviceAdded -= DeviceAdded;
            _deviceService.ExternalDeviceRemoved -= DeviceRemoved;
            _deviceService.Dispose();
            _deviceService = null;
#endif
            Locator.MediaLibrary.MediaListItemAdded -= VLCService_MediaListItemAdded;
            Locator.MediaLibrary.MediaListItemDeleted -= VLCService_MediaListItemDeleted;
            await Locator.MediaLibrary.DisposeDiscoverer();
            _currentStorageVM = null;
            FileExplorersGrouped?.Clear();
        }

        private async Task InitializeSDCard()
        {
            var devices = KnownFolders.RemovableDevices;
            var cards = await devices.GetFoldersAsync();
            if (cards.Any())
            {
                var external = new LocalFileExplorerViewModel(cards[0], RootFolderType.ExternalDevice);
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () => external.LogoGlyph = App.Current.Resources["SDCardSymbol"] as string);
                await AddToFolder(external);
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
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () => external.LogoGlyph = App.Current.Resources["USBFilledSymbol"] as string);
                await AddToFolder(external);
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
                }
            });
        }
#endif

        private async void VLCService_MediaListItemAdded(libVLCX.Media media, int index)
        {
            try
            {
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    var localNetwork = new VLCFileExplorerViewModel(media, RootFolderType.Network);
                    await AddToFolder(localNetwork);
                });
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

        async Task CreateFolderCategory(RootFolderType type)
        {
            var category = new GroupItemList<FileExplorer>() { Key = type };
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () => FileExplorersGrouped.Add(category));
        }

        async Task AddToFolder(FileExplorer fileEx)
        {
            var key = FileExplorersGrouped.FirstOrDefault(x => (RootFolderType)x.Key == fileEx.Type);
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () => key.Add(fileEx));
        }

        public void GoBackToRootFolders()
        {
            CurrentStorageVM = null;
        }
    }
}
