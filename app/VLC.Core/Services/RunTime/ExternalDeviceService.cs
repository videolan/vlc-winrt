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
using Windows.Devices.Enumeration;

namespace VLC.Services.RunTime
{
    public class ExternalDeviceService : IDisposable
    {
        private DeviceWatcher _deviceWatcher;

        public ExternalDeviceService()
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

        public ExternalDeviceAddedEvent ExternalDeviceAdded;
        public ExternalDeviceRemovedEvent ExternalDeviceRemoved;

        public async Task<IEnumerable<string>> GetExternalDeviceIds()
        {
            DeviceInformationCollection devices =
                await DeviceInformation.FindAllAsync(DeviceClass.PortableStorageDevice);

            return devices.Select(d => d.Id);
        }

        private async void DeviceAdded(DeviceWatcher sender, DeviceInformation args)
        {
            await ExternalDeviceAdded(sender, args.Id);
        }

        private async void DeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            await ExternalDeviceRemoved(sender, args.Id);
        }
    }
}
