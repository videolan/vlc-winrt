using VLC_WINRT.Common;
using VLC_WINRT.ViewModels.PlayVideo;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WINRT.Utility.Commands
{
    public class StopVideoCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            var vm = (PlayVideoViewModel) parameter;
            vm.Stop();
            ((Frame) Window.Current.Content).GoBack();
        }
    }
}