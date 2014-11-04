using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MusicPages;
using Windows.UI.Xaml;

namespace VLC_WINRT_APP.Commands.Music
{
    public class ArtistClickedCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            ArtistItem artist = null;
            // artist is clicked from a list
            if (parameter is ItemClickEventArgs)
            {
                ItemClickEventArgs args = parameter as ItemClickEventArgs;
                artist = args.ClickedItem as ArtistItem;
            }
            // artist is selected from a list
            else if (parameter is SelectionChangedEventArgs)
            {
                SelectionChangedEventArgs args = parameter as SelectionChangedEventArgs;
                if (args.AddedItems.Count > 0)
                    artist = args.AddedItems[0] as ArtistItem;
            }
            // searching artist from his id
            else if (parameter is int)
            {
                var id = (int)parameter;
                artist = Locator.MusicLibraryVM.Artists.FirstOrDefault(x => x.Id == id);
            }
            if (artist == null) return;
            Locator.MusicLibraryVM.CurrentArtist = artist;
#if WINDOWS_APP
            if (Window.Current.Bounds.Width < 800)
            {
                App.ApplicationFrame.Navigate(typeof(ArtistPage));
            }
#endif

#if WINDOWS_PHONE_APP
            App.ApplicationFrame.Navigate(typeof(ArtistPage));
#endif
        }
    }
}
