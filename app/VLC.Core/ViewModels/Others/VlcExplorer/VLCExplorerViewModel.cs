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
using VLC.Commands.VLCFileExplorer;
using VLC.Services.RunTime;
using VLC.ViewModels.Others.VlcExplorer;
using System.Threading.Tasks;
using Autofac;
using VLC.Utils;
using VLC.Helpers;
using VLC.Model.FileExplorer;
using Windows.UI.Core;
using Windows.Devices.Portable;
using Windows.UI.Xaml;
using System.Diagnostics;

namespace VLC.ViewModels.RemovableDevicesVM
{
    public class VLCExplorerViewModel : BindableBase
    {
        #region private props
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
                if (DeviceHelper.GetDeviceType() == DeviceTypeEnum.Xbox)
                    categories = categories.Where(x => x != RootFolderType.Library);

                foreach (var c in categories)
                    await CreateFolderCategory(c);

                if (DeviceHelper.GetDeviceType() != DeviceTypeEnum.Xbox)
                {
                    var musicFolder = new LocalFileExplorerViewModel(KnownFolders.MusicLibrary, RootFolderType.Library);
                    musicFolder.LogoGlyph = App.Current.Resources["MusicFilledSymbol"] as string;
                    await AddToFolder(musicFolder);
                    var videoFolder = new LocalFileExplorerViewModel(KnownFolders.VideosLibrary, RootFolderType.Library);
                    videoFolder.LogoGlyph = App.Current.Resources["VideoFilledSymbol"] as string;
                    await AddToFolder(videoFolder);
                    var picFolder = new LocalFileExplorerViewModel(KnownFolders.PicturesLibrary, RootFolderType.Library);
                    picFolder.LogoGlyph = App.Current.Resources["BuddySymbol"] as string;
                    await AddToFolder(picFolder);
                }
            }

            Locator.ExternalDeviceService.ExternalDeviceAdded += DeviceAdded;
            Locator.ExternalDeviceService.ExternalDeviceRemoved += DeviceRemoved;
            
            if (CurrentStorageVM == null)
            {
                FileExplorerVisibility = Visibility.Collapsed;
                RootFoldersVisibility = Visibility.Visible;
            }
            await Task.Run(async () =>
            {
                Locator.MediaLibrary.MediaListItemAdded += VLCService_MediaListItemAdded;
                Locator.MediaLibrary.MediaListItemDeleted += VLCService_MediaListItemDeleted;
                await Locator.MediaLibrary.InitDiscoverer();
            });

            await InitializeUSBKey();
        }

        private async Task InitializeUSBKey()
        {
            await CleanAllFromType(RootFolderType.ExternalDevice);
            var devices = KnownFolders.RemovableDevices;
            IReadOnlyList<StorageFolder> rootFolders = await devices.GetFoldersAsync();
            foreach (StorageFolder rootFolder in rootFolders)
            {
                var external = new LocalFileExplorerViewModel(rootFolder, RootFolderType.ExternalDevice);
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () => external.LogoGlyph = App.Current.Resources["USBFilledSymbol"] as string);
                await AddToFolder(external);
            }
        }

        private async Task InitializeSDCard()
        {
            var devices = KnownFolders.RemovableDevices;
            var cards = await devices.GetFoldersAsync();
            if (cards.Any())
            {
                await CleanAllFromType(RootFolderType.ExternalDevice);
                foreach (var card in cards)
                {
                    var external = new LocalFileExplorerViewModel(card, RootFolderType.ExternalDevice);
                    await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () => external.LogoGlyph = App.Current.Resources["SDCardSymbol"] as string);
                    await AddToFolder(external);
                }
            }
        }

        private async Task DeviceAdded(object sender, string id)
        {
            await InitializeUSBKey();
        }

        private async Task AddFolder(string newId)
        {
            try
            {
                var folder = StorageDevice.FromId(newId);
                var external = new LocalFileExplorerViewModel(folder, RootFolderType.ExternalDevice, newId);
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () => external.LogoGlyph = App.Current.Resources["USBFilledSymbol"] as string);
                await AddToFolder(external);
            }
            catch { }
        }

        private async Task DeviceRemoved(object sender, string id)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                var key = FileExplorersGrouped.FirstOrDefault(x => (RootFolderType)x.Key == RootFolderType.ExternalDevice);
                if (key != null)
                    key.Clear();
            });

            await InitializeUSBKey();
        }

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
            var fileEx = fileExType.ToList().FirstOrDefault(x => x.Name == media.meta(libVLCX.MediaMeta.Title));
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

        async Task CleanAllFromType(RootFolderType type)
        {
            var key = FileExplorersGrouped.FirstOrDefault(x => (RootFolderType)x.Key == type);
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () => { key.Clear(); });
        }

        public void GoBackToRootFolders()
        {
            CurrentStorageVM = null;
        }
    }
}
