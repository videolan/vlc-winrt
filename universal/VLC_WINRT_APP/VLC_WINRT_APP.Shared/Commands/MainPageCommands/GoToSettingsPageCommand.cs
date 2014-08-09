using VLC_WINRT.Common;
using VLC_WINRT_APP.Views.VariousPages;

namespace VLC_WINRT_APP.Commands.MainPageCommands
{
    public class GoToSettingsPageCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            App.ApplicationFrame.Navigate(typeof (SettingsPage));
        }
    }
}
