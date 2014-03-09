using VLC_WINRT.Common;
using VLC_WINRT.Utility.Services.RunTime;

namespace VLC_WINRT.Utility.Commands
{
    public class StopVideoCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            App.RootPage.MainFrame.GoBack();
        }
    }
}