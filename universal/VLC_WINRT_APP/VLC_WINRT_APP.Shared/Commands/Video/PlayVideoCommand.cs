using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.VideoVM;
using VLC_WINRT_APP.Views.VideoPages;

namespace VLC_WINRT_APP.Commands.Video
{
    public class PlayVideoCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (Locator.MusicPlayerVM.IsRunning)
            {
                Locator.MusicPlayerVM.CleanViewModel();
            }
#if WINDOWS_PHONE_APP
            App.NavigationFrame.Navigate(typeof(VideoPlayerPage));
#endif
#if WINDOWS_APP
            if (App.ApplicationFrame.CurrentSourcePageType != typeof(VideoPlayerPage))
                App.ApplicationFrame.Navigate(typeof(VideoPlayerPage));
#endif
            ItemClickEventArgs args = parameter as ItemClickEventArgs;
            VideoVM videoVm = args.ClickedItem as VideoVM;
            string token = StorageApplicationPermissions.FutureAccessList.Add(videoVm.File);
            videoVm.Token = token;
            Locator.PlayVideoVM.CurrentVideo = videoVm;
            Locator.PlayVideoVM.SetActiveVideoInfo(token);
        }
    }
}
