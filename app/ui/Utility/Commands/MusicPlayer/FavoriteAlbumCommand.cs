using VLC_WINRT.Common;
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;

namespace VLC_WINRT.Utility.Commands.MusicPlayer
{
    public class FavoriteAlbumCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            // If the album is favorite, then now it is not
            // if the album was not favorite, now it is
            (parameter as MusicLibraryViewModel.AlbumItem).Favorite = !(parameter as MusicLibraryViewModel.AlbumItem).Favorite;

            // updating the FavoriteAlbums collection
            if((parameter as MusicLibraryViewModel.AlbumItem).Favorite)
                Locator.MusicLibraryVM.FavoriteAlbums.Add(parameter as MusicLibraryViewModel.AlbumItem);
            else if (Locator.MusicLibraryVM.FavoriteAlbums.Contains(parameter as MusicLibraryViewModel.AlbumItem))
                Locator.MusicLibraryVM.FavoriteAlbums.Remove(parameter as MusicLibraryViewModel.AlbumItem);

            // serializing and saving the new Artist collection with updated Favorite property
            Locator.MusicLibraryVM.SerializeArtistsDataBase();
        }
    }
}