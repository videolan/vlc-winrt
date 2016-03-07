using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;

namespace VLC_WinRT.Helpers
{
    public class DeviceStateTrigger : StateTriggerBase
    {
        private DeviceTypeEnum _deviceType;
        private bool _allExcept;

        public bool AllExcept
        {
            get { return _allExcept; }
            set
            {
                _allExcept = value;
                if (_allExcept)
                {
                    var isCurrent = IsCurrentPlatform();
                    SetActive(isCurrent);
                }
            }
        }
        public DeviceTypeEnum DeviceType
        {
            get
            {
                return _deviceType;
            }
            set
            {
                _deviceType = value;
                SetActive(IsCurrentPlatform());
            }
        }

        public bool IsCurrentPlatform()
        {
            return DeviceType == DeviceTypeHelper.GetDeviceType();
        }
    }
}
