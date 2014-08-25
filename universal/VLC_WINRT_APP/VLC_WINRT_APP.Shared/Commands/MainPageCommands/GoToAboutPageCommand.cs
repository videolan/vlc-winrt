using VLC_WINRT.Common;
using VLC_WINRT_APP.Views.VariousPages;

namespace VLC_WINRT_APP.Commands.MainPageCommands
{
    public class GoToAboutPageCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (App.ApplicationFrame.CurrentSourcePageType != typeof (AboutPage))
                App.ApplicationFrame.Navigate(typeof (AboutPage));
        }
    }
}
