using VLC.Model.Video;
using VLC.Utils;
using VLC.ViewModels;
using Windows.UI.Xaml.Controls;

namespace VLC.Commands.Navigation
{
    public class ChangeVideoViewCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.VideoLibraryVM.VideoView = (VideoView)((ItemClickEventArgs)parameter).ClickedItem;
        }
    }
}