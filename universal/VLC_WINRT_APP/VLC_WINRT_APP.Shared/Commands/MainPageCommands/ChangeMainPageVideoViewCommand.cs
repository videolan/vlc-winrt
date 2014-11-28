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
#if WINDOWS_PHONE_APP
            var index = int.Parse(parameter.ToString());
            if (App.ApplicationFrame.CurrentSourcePageType != typeof(MainPageHome)) return;
            if ((App.ApplicationFrame.Content as MainPageHome).MainPivot.SelectedIndex != 1) return;
            switch (index)
            {
                case 0:
                    if ((App.ApplicationFrame.Content as MainPageHome).MainPageVideoPivotItem.MainPageVideoContentPresenter.CurrentSourcePageType != typeof(AllVideosPivotItem))
                    (App.ApplicationFrame.Content as MainPageHome).MainPageVideoPivotItem.MainPageVideoContentPresenter
                        .Navigate(typeof(AllVideosPivotItem));
                    break;
                case 1:
                    if ((App.ApplicationFrame.Content as MainPageHome).MainPageVideoPivotItem.MainPageVideoContentPresenter.CurrentSourcePageType != typeof(ShowsPivotItem))
                    (App.ApplicationFrame.Content as MainPageHome).MainPageVideoPivotItem.MainPageVideoContentPresenter
                        .Navigate(typeof(ShowsPivotItem));
                    break;
                case 2:
                    if ((App.ApplicationFrame.Content as MainPageHome).MainPageVideoPivotItem.MainPageVideoContentPresenter.CurrentSourcePageType != typeof(CameraRollPivotItem))
                    (App.ApplicationFrame.Content as MainPageHome).MainPageVideoPivotItem.MainPageVideoContentPresenter
                        .Navigate(typeof(CameraRollPivotItem));
                    break;
                //case 3:
                //    (App.ApplicationFrame.Content as MainPageHome).MainPageMusicContentPresenter
                //        .Navigate(typeof(PlaylistPivotItem));
                //    break;
            }
#endif
        }
    }
}
