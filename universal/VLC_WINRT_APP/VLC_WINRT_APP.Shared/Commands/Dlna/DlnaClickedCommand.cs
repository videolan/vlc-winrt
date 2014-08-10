using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.Others.VlcExplorer;

namespace VLC_WINRT_APP.Commands.Dlna
{
    public class DlnaClickedCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if ((parameter as SelectionChangedEventArgs).AddedItems.Count != 0)
            {
                FileExplorerViewModel fileExplorer =
                    (parameter as SelectionChangedEventArgs).AddedItems[0] as FileExplorerViewModel;
                Locator.DlnaVM.CurrentDlnaVm = fileExplorer;
                Task.Run(() => Locator.DlnaVM.CurrentDlnaVm.GetFiles());
            }
        }
    }
}
