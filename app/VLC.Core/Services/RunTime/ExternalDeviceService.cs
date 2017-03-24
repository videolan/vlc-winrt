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
using System.Linq;
using System.Threading.Tasks;
using VLC.Commands.ExternalDevice;
using VLC.Model;
using VLC.Model.FileExplorer;
using VLC.Utils;
using VLC.ViewModels;
using VLC.ViewModels.Others.VlcExplorer;
using Windows.Devices.Enumeration;
using Windows.Storage;
using Windows.UI.Core;

namespace VLC.Services.RunTime
{
    public class ExternalDeviceService : IDisposable
    {
        private DeviceWatcher _deviceWatcher;

        public void startWatcher()
        {
            _deviceWatcher = DeviceInformation.CreateWatcher(DeviceClass.PortableStorageDevice);
            _deviceWatcher.Added += DeviceAdded;
            _deviceWatcher.Removed += DeviceRemoved;
            _deviceWatcher.Start();
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

        public delegate Task ExternalDeviceAddedEvent(DeviceWatcher sender, string Id);
        public delegate Task ExternalDeviceRemovedEvent(DeviceWatcher sender, string Id);
        public delegate Task MustIndexExternalDeviceEvent(string Id);
        public delegate Task MustUnindexExternalDeviceEvent();

        public ExternalDeviceAddedEvent ExternalDeviceAdded;
        public ExternalDeviceRemovedEvent ExternalDeviceRemoved;
        public MustIndexExternalDeviceEvent MustIndexExternalDevice;
        public MustUnindexExternalDeviceEvent MustUnindexExternalDevice;

        private async void DeviceAdded(DeviceWatcher sender, DeviceInformation args)
        {
            switch (Locator.SettingsVM.ExternalDeviceMode)
            {
                case ExternalDeviceMode.AskMe:
                    await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal,
                        () => new ShowExternalDevicePage().Execute(args));
                    break;
                case ExternalDeviceMode.IndexMedias:
                    await AskExternalDeviceIndexing(args.Id);
                    break;
                case ExternalDeviceMode.SelectMedias:
                    await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, async () =>
                    {
                        await AskContentToCopy(args.Id);
                    });
                    break;
                case ExternalDeviceMode.DoNothing:
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (ExternalDeviceAdded != null)
                await ExternalDeviceAdded(sender, args.Id);
        }

        public async Task AskExternalDeviceIndexing(string deviceId)
        {
            if (MustIndexExternalDevice != null)
                await MustIndexExternalDevice(deviceId);
        }

        public async Task AskContentToCopy(string deviceId)
        {
            // Display the folder of the first external storage device detected.
            Locator.MainVM.CurrentPanel = Locator.MainVM.Panels.FirstOrDefault(x => x.Target == VLCPage.MainPageFileExplorer);

            StorageFolder rootFolder;
            if (Helpers.DeviceHelper.GetDeviceType() != Helpers.DeviceTypeEnum.Xbox &&
                Helpers.DeviceHelper.GetDeviceType() != Helpers.DeviceTypeEnum.Phone)
                rootFolder = Windows.Devices.Portable.StorageDevice.FromId(deviceId);
            else
            {
                var devices = KnownFolders.RemovableDevices;
                var allFolders = await devices.GetFoldersAsync();
                rootFolder = allFolders.Last();
            }
            if (rootFolder == null)
                return;

            var storageItem = new VLCStorageFolder(rootFolder);
            Locator.FileExplorerVM.CurrentStorageVM = new LocalFileExplorerViewModel(
                rootFolder, RootFolderType.ExternalDevice);
            await Locator.FileExplorerVM.CurrentStorageVM.GetFiles();
        }

        private async void DeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            if (ExternalDeviceRemoved != null)
                await ExternalDeviceRemoved(sender, args.Id);

            if (MustUnindexExternalDevice != null)
                await MustUnindexExternalDevice();
        }
    }
}
