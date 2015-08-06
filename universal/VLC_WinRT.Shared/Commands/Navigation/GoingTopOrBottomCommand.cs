using ScrollWatchedSelector;
using VLC_WinRT.Model;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.Navigation
{
    public class GoingTopOrBottomCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            ScrollingEventArgs lv = parameter as ScrollingEventArgs;
            if (lv.ScrollingType == ScrollingType.ToBottom)
            {
                if (Locator.MediaPlaybackViewModel.TrackCollection.IsRunning && Locator.MediaPlaybackViewModel.PlayingType == PlayingType.Music)
                    App.SplitShell.HideTopBar();
            }
            else if (lv.ScrollingType == ScrollingType.ToTop)
            {
                App.SplitShell.ShowTopBar();
            }
        }
    }
}
