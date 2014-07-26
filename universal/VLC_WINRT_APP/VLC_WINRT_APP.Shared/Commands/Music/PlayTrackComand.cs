/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Linq;
using System.Threading.Tasks;
using Windows.Globalization.DateTimeFormatting;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Converters;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;
using VLC_WINRT_APP.Views.MusicPages;

namespace VLC_WINRT_APP.Commands.Music
{
    public class TrackClickedCommand : AlwaysExecutableCommand
    {
        public async override void Execute(object parameter)
        {
            App.RootPage.MainFrameThemeTransition.Edge = EdgeTransitionLocation.Right;
            if (App.ApplicationFrame.CurrentSourcePageType != typeof(MusicPlayerPage))
                App.ApplicationFrame.Navigate(typeof(MusicPlayerPage));
            Locator.MusicLibraryVM.IsAlbumPageShown = false;
            MusicLibraryVM.TrackItem track = null;
            if (parameter is ItemClickEventArgs)
            {
                ItemClickEventArgs args = parameter as ItemClickEventArgs;
                track = args.ClickedItem as MusicLibraryVM.TrackItem;
            }
            MusicLibraryVM.AlbumItem album =
                Locator.MusicLibraryVM.Artists.FirstOrDefault(x => x.Id == track.ArtistId)
                    .Albums.FirstOrDefault(x => x.Id == track.AlbumId);
            if (album != null)
            {
                Task.Run(() => album.Play(track.Index-1));
            }
            else
            {
                Task.Run(() => track.Play());
            }
#if WINDOWS_APP
            //else if (parameter is DataGridSelectionChangedEventArgs)
            //{
            //    DataGridSelectionChangedEventArgs args = parameter as DataGridSelectionChangedEventArgs;
            //    track = args.AddedItems.First() as MusicLibraryVM.TrackItem;
            //}
#endif
        }
    }
}