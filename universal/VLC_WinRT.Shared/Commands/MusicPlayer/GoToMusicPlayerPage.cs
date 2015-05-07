using VLC_WinRT.Model;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.MusicPlayer
{
    public class GoToMusicPlayerPage : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.NavigationService.Go(VLCPage.MusicPlayerPage);
        }
    }
}
