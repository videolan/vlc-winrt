using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;

namespace VLC.Helpers
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
                ComputeActive();
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
                ComputeActive();
            }
        }

        public void ComputeActive()
        {
            var isCurrent = IsCurrentPlatform();
            var active = (AllExcept) ? !isCurrent : isCurrent;
            SetActive(active);
        }

        public bool IsCurrentPlatform()
        {
            return DeviceType == DeviceHelper.GetDeviceType();
        }
    }
}
