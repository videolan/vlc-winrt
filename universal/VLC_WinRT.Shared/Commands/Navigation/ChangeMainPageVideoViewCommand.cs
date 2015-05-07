using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Views.MainPages;
using VLC_WinRT.Views.MainPages.MainVideoControls;
using Windows.UI.Xaml.Controls;

namespace VLC_WinRT.Commands.Navigation
{
    public class ChangeMainPageVideoViewCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            var index = int.Parse(parameter.ToString());
            var frame = (App.ApplicationFrame.Content as MainPageVideos).MainPageVideoContentPresenter;
            Switch(index, frame);
        }

        void Switch(int index, ContentPresenter frame)
        {
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
                    if (frame.Content is SearchVideoPage)
                    {
                        Switch((int)Locator.SettingsVM.VideoView, frame);
                        Locator.VideoLibraryVM.SearchTag = "";
                    }
                    else
                        frame.Content = new SearchVideoPage();
                    break;
            }
        }
    }
}

