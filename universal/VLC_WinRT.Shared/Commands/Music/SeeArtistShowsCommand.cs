using System.Linq;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Model;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Commands.Music
{
    public class SeeArtistShowsCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.NavigationService.Go(VLCPage.ArtistShowsPage);
            var artistItem = parameter as ArtistItem;
            if (artistItem == null && parameter is int)
            {
                artistItem = Locator.MusicLibraryVM.Artists.FirstOrDefault(x => x.Id == (int)parameter);
                if (artistItem == null) return;
            }
            Locator.MusicLibraryVM.CurrentArtist = artistItem;
        }
    }
}