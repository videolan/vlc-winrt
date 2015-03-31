using VLC_WINRT.Common;
using VLC_WinRT.Views.MainPages;
using VLC_WinRT.Views.MainPages.MainVideoControls;

namespace VLC_WinRT.Commands.MainPageCommands
{
    public class ChangeMainPageVideoViewCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            var index = int.Parse(parameter.ToString());
            var frame = (App.ApplicationFrame.Content as MainPageVideos).MainPageVideoContentPresenter;
            switch (index)
            {
                case 0:
                    if (!(frame.Content is AllVideosPivotItem))
                        frame.Content = new AllVideosPivotItem();
                    break;
                case 1:
                    if (!(frame.Content is ShowsPivotItem))
                        frame.Content = new ShowsPivotItem();
                    break;
                case 2:
                    if (!(frame.Content is CameraRollPivotItem))
                        frame.Content = new CameraRollPivotItem();
                    break;
                case 3:
                    if (!(frame.Content is SearchVideoPage))
                        frame.Content = new SearchVideoPage();
                    break;
            }
        }
    }
}

