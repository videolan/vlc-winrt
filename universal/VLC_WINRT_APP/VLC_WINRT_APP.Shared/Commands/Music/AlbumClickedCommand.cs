using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;
using VLC_WINRT_APP.Views.MusicPages;

namespace VLC_WINRT_APP.Commands.Music
{
    public class AlbumClickedCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            App.RootPage.MainFrameThemeTransition.Edge = EdgeTransitionLocation.Right;
            App.ApplicationFrame.Navigate(typeof(MusicPlayerPage));
            Locator.MusicLibraryVM.IsAlbumPageShown = false;
            AlbumItem album = parameter as AlbumItem;
            
            if (album == null)
            {
                ItemClickEventArgs args = parameter as ItemClickEventArgs;
                album = args.ClickedItem as AlbumItem;
            }

            Locator.MusicLibraryVM.CurrentArtist =
                Locator.MusicLibraryVM.Artists.FirstOrDefault(x => x.Name == album.Artist);
            //PlayMusickHelper.AddToQueue(album);
        }
    }
}
