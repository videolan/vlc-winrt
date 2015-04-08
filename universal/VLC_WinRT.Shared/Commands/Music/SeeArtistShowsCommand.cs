using System.Linq;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Views.MusicPages.ArtistPages;
using VLC_WinRT.Model;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Commands.Music
{
    public class SeeArtistShowsCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.NavigationService.Go(VLCPage.ArtistShowsPage);
            if (parameter is ArtistItem)
            {
                (App.ApplicationFrame.Content as ArtistShowsPage).DataContext = parameter as ArtistItem;
            }
            else if (parameter is int)
            {
                var artistItem = Locator.MusicLibraryVM.Artists.FirstOrDefault(x => x.Id == (int) parameter);
                if (artistItem == null) return;
                (App.ApplicationFrame.Content as ArtistShowsPage).DataContext = artistItem;
            }
        }
    }
}