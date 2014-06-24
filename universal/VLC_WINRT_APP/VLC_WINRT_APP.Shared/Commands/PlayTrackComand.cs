/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.UI.Xaml.Media.Animation;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Services.RunTime;
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT_APP;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MainPages;
using VLC_WINRT_APP.Views.MusicPages;
#if NETFX_CORE
using VLC_WINRT.Views;
using VLC_WINRT_APP.ViewModels.MusicVM;
using Windows.UI.Xaml.Controls;
#endif
#if WINDOWS_PHONE_APP
using VLC_WINPRT;
#endif

namespace VLC_WINRT_APP.Commands
{
    public class PlayTrackCommand : AlwaysExecutableCommand
    {
        public async override void Execute(object parameter)
        {
#if WINDOWS_PHONE_APP
            if (App.NavigationFrame.CurrentSourcePageType != typeof(MusicPlayerPage))
                App.NavigationFrame.Navigate(typeof(MusicPlayerPage));
#endif
#if WINDOWS_APP
            if (App.ApplicationFrame.CurrentSourcePageType != typeof(MusicPlayerPage))
                App.ApplicationFrame.Navigate(typeof(MusicPlayerPage));
#endif
            Locator.MusicLibraryVM.IsAlbumPageShown = false;
            MusicLibraryVM.TrackItem track = null;
            if (parameter is ItemClickEventArgs)
            {
                ItemClickEventArgs args = parameter as ItemClickEventArgs;
                track = args.ClickedItem as MusicLibraryVM.TrackItem;
            }
#if WINDOWS_APP
            //else if (parameter is DataGridSelectionChangedEventArgs)
            //{
            //    DataGridSelectionChangedEventArgs args = parameter as DataGridSelectionChangedEventArgs;
            //    track = args.AddedItems.First() as MusicLibraryVM.TrackItem;
            //}
#endif
            if (track != null && !Locator.MusicPlayerVM.TrackCollection.Contains(track))
            {
                Locator.MusicPlayerVM.ResetCollection();
                Locator.MusicPlayerVM.AddTrack(track);
            }
            else
            {
                Locator.MusicPlayerVM.CurrentTrack =
                    Locator.MusicPlayerVM.TrackCollection.IndexOf(track);
            }
            await Locator.MusicPlayerVM.Play();
        }
    }
}
