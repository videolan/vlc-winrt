using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Commands.Video
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
