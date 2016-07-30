using VLC.Helpers;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.VideoPlayer
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
