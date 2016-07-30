using Windows.UI.Xaml.Controls;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.VideoLibrary
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
