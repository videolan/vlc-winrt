using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.Commands.Music
{
    public class ArtistClickedCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
#if WINDOWS_PHONE_APP
    //App.RootFrame.Navigate(typeof(ArtistPage));
#endif
            MusicLibraryVM.ArtistItem artist = null;
            if (parameter is ItemClickEventArgs)
            {
                ItemClickEventArgs args = parameter as ItemClickEventArgs;
                artist = args.ClickedItem as MusicLibraryVM.ArtistItem;
            }
            else if (parameter is SelectionChangedEventArgs)
            {
                SelectionChangedEventArgs args = parameter as SelectionChangedEventArgs;
                if(args.AddedItems.Count > 0)
                    artist = args.AddedItems[0] as MusicLibraryVM.ArtistItem;
            }
            if (artist != null)
                Locator.MusicLibraryVM.CurrentArtist = artist;
        }
    }
}
