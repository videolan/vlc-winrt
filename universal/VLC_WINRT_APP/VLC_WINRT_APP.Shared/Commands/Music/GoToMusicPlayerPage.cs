using VLC_WINRT.Common;
using VLC_WinRT.Views.MusicPages;

namespace VLC_WinRT.Commands.Music
{
    public class GoToMusicPlayerPage : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if(App.ApplicationFrame.CurrentSourcePageType != typeof(MusicPlayerPage))
                App.ApplicationFrame.Navigate(typeof (MusicPlayerPage));
        }
    }
}
