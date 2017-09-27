using System.Linq;
using Windows.UI.Xaml.Controls;
using VLC.Model.Music;
using VLC.ViewModels;
using VLC.Model;
using VLC.Utils;

namespace VLC.Commands.MusicLibrary
{
    public class ArtistClickedCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
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
                artist = Locator.MediaLibrary.LoadArtist(id);
            }
            if (artist == null) return;
            Locator.MusicLibraryVM.CurrentArtist = artist;
            Locator.MusicLibraryVM.MusicView = MusicView.Artists;
            Locator.NavigationService.Go(VLCPage.ArtistShowsPage);
        }
    }
}
