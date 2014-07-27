using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using VLC_WINRT.Common;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MainPages;
using VLC_WINRT_APP.ViewModels.MusicVM;
using VLC_WINRT_APP.Views.MusicPages;

namespace VLC_WINRT_APP.Commands.Music
{
    public class AlbumClickedCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            App.RootPage.MainFrameThemeTransition.Edge = EdgeTransitionLocation.Right;
            App.ApplicationFrame.Navigate(typeof(AlbumPage));
            Locator.MusicLibraryVM.IsAlbumPageShown = true;
            ItemClickEventArgs args = parameter as ItemClickEventArgs;
            MusicLibraryVM.AlbumItem album = args.ClickedItem as MusicLibraryVM.AlbumItem;
            Locator.MusicLibraryVM.CurrentArtist =
                Locator.MusicLibraryVM.Artists.FirstOrDefault(x => x.Name == album.Artist);
            if (Locator.MusicLibraryVM.CurrentArtist != null)
                Locator.MusicLibraryVM.CurrentArtist.CurrentAlbumIndex = Locator.MusicLibraryVM.CurrentArtist.Albums.IndexOf(album);
        }
    }
}
