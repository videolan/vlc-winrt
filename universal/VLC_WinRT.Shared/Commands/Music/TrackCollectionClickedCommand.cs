using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WinRT.ViewModels;
using VLC_WinRT.ViewModels.MusicVM;
using VLC_WinRT.Views.MusicPages;

namespace VLC_WinRT.Commands.Music
{
    public class TrackCollectionClickedCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (parameter is TrackCollection)
            {
                Locator.MusicLibraryVM.CurrentTrackCollection = parameter as TrackCollection;
                App.ApplicationFrame.Navigate(typeof (PlaylistPage));
            }
            else if (parameter is SelectionChangedEventArgs)
            {
                Locator.MusicLibraryVM.CurrentTrackCollection =
                    (parameter as SelectionChangedEventArgs).AddedItems[0] as TrackCollection;
                App.ApplicationFrame.Navigate(typeof (PlaylistPage));
            }
            else if (parameter is ItemClickEventArgs)
            {
                Locator.MusicLibraryVM.CurrentTrackCollection =
                    (parameter as ItemClickEventArgs).ClickedItem as TrackCollection;
                App.ApplicationFrame.Navigate(typeof (PlaylistPage));
            }
        }
    }
}
