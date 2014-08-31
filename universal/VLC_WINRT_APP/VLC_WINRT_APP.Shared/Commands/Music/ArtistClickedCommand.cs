using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using VLC_WINRT.Common;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MainPages;
using VLC_WINRT_APP.Views.MusicPages;
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
            ItemClickEventArgs args = parameter as ItemClickEventArgs;
            MusicLibraryVM.ArtistItem artist = args.ClickedItem as MusicLibraryVM.ArtistItem;
            Locator.MusicLibraryVM.CurrentArtist = artist;
        }
    }
}
