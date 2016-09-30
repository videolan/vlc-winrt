using System.Linq;
using VLC.Model.Music;
using VLC.ViewModels;
using VLC.Model;
using VLC.Utils;

namespace VLC.Commands.MusicLibrary
{
    public class SeeArtistShowsCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.NavigationService.Go(VLCPage.ArtistShowsPage);
            var artistItem = parameter as ArtistItem;
            if (artistItem == null && parameter is int)
            {
                artistItem = Locator.MediaLibrary.LoadArtist((int)parameter);
                if (artistItem == null) return;
            }
            Locator.MusicLibraryVM.CurrentArtist = artistItem;
        }
    }
}