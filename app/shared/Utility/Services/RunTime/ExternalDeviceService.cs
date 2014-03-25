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

namespace VLC_WINRT.Utility.Services.RunTime
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

        public event EventHandler<string> ExternalDeviceAdded;
        public event EventHandler<string> ExternalDeviceRemoved;

        public async Task<IEnumerable<string>> GetExternalDeviceIds()
        {
            DeviceInformationCollection devices =
                await DeviceInformation.FindAllAsync(DeviceClass.PortableStorageDevice);

            return devices.Select(d => d.Id);
        }

        private void DeviceAdded(DeviceWatcher sender, DeviceInformation args)
        {
            if (ExternalDeviceAdded != null)
            {
                ExternalDeviceAdded(this, args.Id);
            }
        }

        private void DeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            if (ExternalDeviceRemoved != null)
            {
                ExternalDeviceRemoved(this, args.Id);
            }
        }
    }
}
