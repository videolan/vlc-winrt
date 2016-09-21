using VLC.Model.Music;
using VLC.Utils;
using VLC.ViewModels;
using Windows.UI.Xaml.Controls;

namespace VLC.Commands.Navigation
{
    public class ChangeMusicViewCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.MusicLibraryVM.MusicView = (MusicView)((ItemClickEventArgs)parameter).ClickedItem;
        }
    }
}
