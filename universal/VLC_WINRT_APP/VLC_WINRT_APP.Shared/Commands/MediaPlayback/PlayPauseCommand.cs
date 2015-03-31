/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using Windows.UI.Core;
#if WINDOWS_PHONE_APP
using Windows.Media.Playback;
#endif
using Autofac;
using VLC_WINRT.Common;
using VLC_WinRT.Model;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.MediaPlayback
{
    public class PlayPauseCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
#if WINDOWS_APP
            Locator.MediaPlaybackViewModel._mediaService.Pause();
#else
            if (BackgroundMediaPlayer.Current != null && Locator.MediaPlaybackViewModel.PlayingType == PlayingType.Music && !Locator.MediaPlaybackViewModel.UseVlcLib)
            {
                switch (BackgroundMediaPlayer.Current.CurrentState)
                {
                    case MediaPlayerState.Closed:
                        await Locator.MediaPlaybackViewModel.SetMedia(Locator.MusicPlayerVM.CurrentTrack, false);
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
                    Locator.MediaPlaybackViewModel._mediaService.Pause();
                });
            }
#endif
        }
    }
}
