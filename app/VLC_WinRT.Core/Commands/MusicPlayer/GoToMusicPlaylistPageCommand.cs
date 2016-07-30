using VLC.ViewModels;
using VLC.Model;
using VLC.Utils;

namespace VLC.Commands.MusicPlayer
{
    public class GoToMusicPlaylistPageCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (Locator.NavigationService.CurrentPage != VLCPage.MusicPlayerPage)
                Locator.NavigationService.Go(VLCPage.MusicPlayerPage);
            else
                Locator.NavigationService.Go(VLCPage.CurrentPlaylistPage);
        }
    }
}
