using VLC_WinRT.Helpers;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.VideoPlayer
{
    public class SurfaceZoomToggleCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            var index = (int)Locator.VideoPlayerVm.CurrentSurfaceZoom;
            var nextIndex = ++index;
            if (nextIndex == Locator.VideoPlayerVm.Zooms.Count)
                nextIndex = 0;
            Locator.VideoPlayerVm.CurrentSurfaceZoom = Locator.VideoPlayerVm.Zooms[nextIndex];
        }
    }
}
