using VLC_WINRT.Common;
using VLC_WINRT_APP.Views.MusicPages;

namespace VLC_WINRT_APP.Commands.Music
{
    public class GoToMusicPlaylistPageCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            App.ApplicationFrame.Navigate(typeof (MusicPlaylistPage));
        }
    }
}
