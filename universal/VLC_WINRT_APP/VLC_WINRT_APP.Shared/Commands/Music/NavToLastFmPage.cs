using VLC_WINRT.Common;
using VLC_WINRT_APP.Views.UserControls;

namespace VLC_WINRT_APP.Commands.Music
{
    public class NavToLastFmPage : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            App.ApplicationFrame.Navigate(typeof (ConnectToLastFm));
        }
    }
}
