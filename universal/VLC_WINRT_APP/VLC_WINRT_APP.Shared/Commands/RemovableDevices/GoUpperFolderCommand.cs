using VLC_WINRT.Common;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MainPages;

namespace VLC_WINRT_APP.Commands.RemovableDevices
{
    public class GoUpperFolderCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (App.ApplicationFrame.CurrentSourcePageType == typeof (MainPageRemovables))
            {
                if (Locator.ExternalStorageVM.CurrentStorageVM != null &&
                    Locator.ExternalStorageVM.CurrentStorageVM.CanGoBack)
                    Locator.ExternalStorageVM.CurrentStorageVM.GoBack();
            }
#if WINDOWS_APP
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
