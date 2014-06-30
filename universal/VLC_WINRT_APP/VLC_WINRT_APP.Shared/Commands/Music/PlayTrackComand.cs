/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;
using VLC_WINRT_APP.Views.MusicPages;

namespace VLC_WINRT_APP.Commands.Music
{
    public class PlayTrackCommand : AlwaysExecutableCommand
    {
        public async override void Execute(object parameter)
        {
            if (App.ApplicationFrame.CurrentSourcePageType != typeof(MusicPlayerPage))
                App.ApplicationFrame.Navigate(typeof(MusicPlayerPage));
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
