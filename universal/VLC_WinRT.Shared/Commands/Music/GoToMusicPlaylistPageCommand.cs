using VLC_WINRT.Common;
using VLC_WinRT.Views.MusicPages;

namespace VLC_WinRT.Commands.Music
{
    public class GoToMusicPlaylistPageCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            App.ApplicationFrame.Navigate(typeof (MusicPlaylistPage));
        }
    }
}
