using VLC_WINRT.Common;
using VLC_WINRT_APP.Views.MainPages;
using VLC_WINRT_APP.Views.MainPages.MainMusicControls;
using VLC_WINRT_APP.Views.MainPages.MainVideoControls;

namespace VLC_WINRT_APP.Commands.MainPageCommands
{
    public class ChangeMainPageVideoViewCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            var index = int.Parse(parameter.ToString());
#if WINDOWS_PHONE_APP
            if (App.ApplicationFrame.CurrentSourcePageType != typeof(MainPageHome)) return;
            if ((App.ApplicationFrame.Content as MainPageHome).MainPivot.SelectedIndex != 1) return;
#endif
#if WINDOWS_PHONE_APP
            var frame = (App.ApplicationFrame.Content as MainPageHome).MainPageVideoPivotItem.MainPageVideoContentPresenter;
#else
            var frame = (App.ApplicationFrame.Content as MainPageVideos).MainPageVideoContentPresenter;
#endif
            switch (index)
            {
                case 0:
                    if (frame.CurrentSourcePageType != typeof(AllVideosPivotItem))
                        frame.Navigate(typeof(AllVideosPivotItem));
                    break;
                case 1:
                    if (frame.CurrentSourcePageType != typeof(ShowsPivotItem))
                        frame.Navigate(typeof(ShowsPivotItem));
                    break;
                case 2:
                    if (frame.CurrentSourcePageType != typeof(CameraRollPivotItem))
                        frame.Navigate(typeof(CameraRollPivotItem));
                    break;
                //case 3:
                //    (App.ApplicationFrame.Content as MainPageHome).MainPageMusicContentPresenter
                //        .Navigate(typeof(PlaylistPivotItem));
                //    break;
            }
        }
    }
}

