using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Model.Video;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.VideoPages;
using VLC_WINRT_APP.Helpers.VideoPlayer;

namespace VLC_WINRT_APP.Commands.Video
{
    public class PlayVideoCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (Locator.MusicPlayerVM.TrackCollection.IsRunning)
            {
                Locator.MusicPlayerVM.CleanViewModel();
            }
            try
            {
                if (App.ApplicationFrame.CurrentSourcePageType != typeof (VideoPlayerPage))
                    App.ApplicationFrame.Navigate(typeof (VideoPlayerPage));
            }
            catch { }
            VideoItem videoVm = null;
            if (parameter is ItemClickEventArgs)
            {
                ItemClickEventArgs args = parameter as ItemClickEventArgs;
                videoVm = args.ClickedItem as VideoItem;
            }
            else if(parameter is VideoItem)
            {
                videoVm = parameter as VideoItem;
            }
            videoVm.Play();
        }
    }
}
