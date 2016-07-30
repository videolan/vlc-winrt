using ScrollWatcher;
using VLC.Model;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.Navigation
{
    public class ScrollDetectedCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            ScrollingEventArgs lv = parameter as ScrollingEventArgs;
            if (lv.ScrollDirection == ScrollDirection.Bottom)
            {
                if ((Locator.MediaPlaybackViewModel.PlaybackService.IsRunning 
                    && Locator.MediaPlaybackViewModel.PlaybackService.PlayingType == PlayingType.Music)
                    || Locator.NavigationService.CurrentPage == VLCPage.ArtistPage
                    || Locator.NavigationService.CurrentPage == VLCPage.ArtistInfoView)
                {
                    App.SplitShell.HideTopBar();
                }
            }
            else if (lv.ScrollDirection == ScrollDirection.Top)
            {
                App.SplitShell?.ShowTopBar();
            }
        }
    }
}
