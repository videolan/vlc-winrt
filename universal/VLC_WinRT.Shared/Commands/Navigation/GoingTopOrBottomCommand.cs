using ScrollWatcher;
using VLC_WinRT.Model;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.Navigation
{
    public class ScrollDetectedCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            ScrollingEventArgs lv = parameter as ScrollingEventArgs;
            if (lv.ScrollDirection == ScrollDirection.Bottom)
            {
                if (Locator.MediaPlaybackViewModel.TrackCollection.IsRunning && Locator.MediaPlaybackViewModel.PlayingType == PlayingType.Music)
                    App.SplitShell.HideTopBar();
            }
            else if (lv.ScrollDirection == ScrollDirection.Top)
            {
                App.SplitShell.ShowTopBar();
            }
        }
    }
}
