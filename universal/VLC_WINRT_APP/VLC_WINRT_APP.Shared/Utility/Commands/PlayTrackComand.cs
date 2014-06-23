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
using VLC_WINRT_APP.Utility.Helpers;
using VLC_WINRT_APP.Utility.Services.RunTime;
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT_APP;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MainPage;
using VLC_WINRT_APP.Views.MainPages;
using VLC_WINRT_APP.Views.MusicPages;
#if NETFX_CORE
using VLC_WINRT.Views;
#endif
#if WINDOWS_PHONE_APP
using VLC_WINPRT;
#endif

namespace VLC_WINRT_APP.Utility.Commands
{
    public class PlayTrackCommand : AlwaysExecutableCommand
    {
        public async override void Execute(object parameter)
        {
            MusicLibraryVM.TrackItem track = parameter as MusicLibraryVM.TrackItem;
            if (!Locator.MusicPlayerVM.TrackCollection.TrackCollection.Contains(track))
            {
                Locator.MusicPlayerVM.TrackCollection.ResetCollection();
                Locator.MusicPlayerVM.TrackCollection.AddTrack(track);
            }
            else
            {
                Locator.MusicPlayerVM.TrackCollection.CurrentTrack =
                    Locator.MusicPlayerVM.TrackCollection.TrackCollection.IndexOf(track);
            }
            await Locator.MusicPlayerVM.Play();

            var frame = App.ApplicationFrame;
#if NETFX_CORE
            var page = frame.Content as MainPage;
            if (page != null)
            {
                var sB = page.Resources["FadeOutPage"] as Storyboard;
                if (sB != null)
                {
                    await sB.BeginAsync();
                    App.ApplicationFrame.Navigate(typeof(ArtistPage));
                }
            } 
#endif
        }
    }
}
