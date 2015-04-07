using VLC_WINRT.Common;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Model;

namespace VLC_WinRT.Commands.Music
{
    public class GoToMusicPlaylistPageCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (Locator.MainVM.NavigationService.CurrentPage != VLCPage.MusicPlayerPage)
                Locator.MainVM.NavigationService.Go(VLCPage.MusicPlayerPage);
            else
                Locator.MainVM.NavigationService.Go(VLCPage.CurrentPlaylistPage);
        }
    }
}
