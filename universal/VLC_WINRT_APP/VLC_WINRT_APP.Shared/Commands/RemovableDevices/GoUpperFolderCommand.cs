using System;
using System.Collections.Generic;
using System.Text;
using VLC_WINRT.Common;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MainPages;

namespace VLC_WINRT_APP.Commands.RemovableDevices
{
    public class GoUpperFolderCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
#if WINDOWS_APP
            if (App.ApplicationFrame.CurrentSourcePageType == typeof (MainPageRemovables))
            {
                if (Locator.ExternalStorageVM.CurrentStorageVM != null &&
                    Locator.ExternalStorageVM.CurrentStorageVM.CanGoBack)
                    Locator.ExternalStorageVM.CurrentStorageVM.GoBack();
            }
            else if(App.ApplicationFrame.CurrentSourcePageType == typeof(MainPageMediaServers))
            {
                if(Locator.DlnaVM.CurrentDlnaVm != null &&
                    Locator.DlnaVM.CurrentDlnaVm.CanGoBack)
                    Locator.DlnaVM.CurrentDlnaVm.GoBack();
            }
#endif
        }
    }
}
