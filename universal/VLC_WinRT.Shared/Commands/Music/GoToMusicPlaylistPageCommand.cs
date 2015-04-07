using VLC_WINRT.Common;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Model;

namespace VLC_WinRT.Commands.Music
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
