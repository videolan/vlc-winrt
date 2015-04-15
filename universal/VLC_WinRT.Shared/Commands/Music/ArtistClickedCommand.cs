using System.Linq;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Model;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Commands.Music
{
    public class ArtistClickedCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.NavigationService.Go(VLCPage.ArtistPage);
            ArtistItem artist = null;
            if (parameter is ArtistItem)
            {
                artist = (ArtistItem)parameter;
            }
            // artist is clicked from a list
            else if (parameter is ItemClickEventArgs)
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
        }
    }
}
