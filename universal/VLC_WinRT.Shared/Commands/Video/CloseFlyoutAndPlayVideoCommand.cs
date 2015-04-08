using Windows.UI.Xaml.Controls;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.Video
{
    public class CloseFlyoutAndPlayVideoCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            var videoFlyout = App.Current.Resources["VideoInformationFlyout"] as Flyout;
            videoFlyout?.Hide();
            Locator.VideoLibraryVM.OpenVideo.Execute(parameter);
        }
    }
}
