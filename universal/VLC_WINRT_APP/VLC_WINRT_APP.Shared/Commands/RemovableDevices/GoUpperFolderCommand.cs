using System;
using System.Collections.Generic;
using System.Text;
using VLC_WINRT.Common;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Commands.RemovableDevices
{
    public class GoUpperFolderCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if(Locator.ExternalStorageVM.CurrentStorageVM.CanGoBack)
                Locator.ExternalStorageVM.CurrentStorageVM.GoBack();
        }
    }
}
