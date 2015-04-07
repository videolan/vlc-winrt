using VLC_WINRT.Common;
using VLC_WinRT.Views.MusicPages;
using VLC_WinRT.Model;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.Music
{
    public class GoToMusicPlayerPage : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.MainVM.NavigationService.Go(VLCPage.MusicPlayerPage);
        }
    }
}
