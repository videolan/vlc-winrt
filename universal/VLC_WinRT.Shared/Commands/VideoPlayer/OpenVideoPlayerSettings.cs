using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.VideoPlayer
{
    public class OpenVideoPlayerOptionsPanel : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.NavigationService.Go(Model.VLCPage.VideoPlayerOptionsPanel);
        }
    }
}
