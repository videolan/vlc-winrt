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
using VLC.Model;
using VLC.Utils;
using VLC.ViewModels;
using Windows.Devices.Enumeration;
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
        public delegate Task MustIndexExternalDeviceEvent();
        public delegate Task MustUnindexExternalDeviceEvent();

        public ExternalDeviceAddedEvent ExternalDeviceAdded;
        public ExternalDeviceRemovedEvent ExternalDeviceRemoved;
        public MustIndexExternalDeviceEvent MustIndexExternalDevice;
        public MustUnindexExternalDeviceEvent MustUnindexExternalDevice;

        public async Task<IEnumerable<string>> GetExternalDeviceIds()
        {
            DeviceInformationCollection devices =
                await DeviceInformation.FindAllAsync(DeviceClass.PortableStorageDevice);

            return devices.Select(d => d.Id);
        }

        private async void DeviceAdded(DeviceWatcher sender, DeviceInformation args)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.High,
                () => Locator.NavigationService.Go(VLCPage.ExternalStorageInclude));

            if (ExternalDeviceAdded != null)
                await ExternalDeviceAdded(sender, args.Id);
        }

        public async void AskExternalDeviceIndexing()
        {
            if (MustIndexExternalDevice != null)
                await MustIndexExternalDevice();
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
