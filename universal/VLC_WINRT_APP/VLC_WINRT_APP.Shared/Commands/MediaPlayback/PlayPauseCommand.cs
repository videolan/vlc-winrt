/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using Windows.Media.Playback;
using Windows.UI.Core;
#if WINDOWS_PHONE_APP
using Windows.Media.Playback;
#endif
using Autofac;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Helpers.MusicLibrary.LastFm;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Services.Interface;
using VLC_WINRT_APP.Services.RunTime;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Commands.MediaPlayback
{
    public class PlayPauseCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var playerService = App.Container.Resolve<IMediaService>();
#if WINDOWS_APP
            playerService.Pause();
#else
            if (BackgroundMediaPlayer.Current != null && Locator.MusicPlayerVM.PlayingType == PlayingType.Music && !playerService.UseVlcLib)
            {
                switch (BackgroundMediaPlayer.Current.CurrentState)
                {
                    case MediaPlayerState.Closed:
                        await Locator.MusicPlayerVM.Play(false);
                        App.BackgroundAudioHelper.AddMediaPlayerEventHandlers();
                        break;
                    case MediaPlayerState.Paused:
                        BackgroundMediaPlayer.Current.Play();
                        break;
                    case MediaPlayerState.Playing:
                        BackgroundMediaPlayer.Current.Pause();
                        break;
                }
            }
            else
            {
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (playerService != null) playerService.Pause();
                });
            }
#endif
        }
    }
}
