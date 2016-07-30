using VLC.Model;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.MusicPlayer
{
    public class GoToMusicPlayerPage : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.NavigationService.Go(VLCPage.MusicPlayerPage);
        }
    }
}
