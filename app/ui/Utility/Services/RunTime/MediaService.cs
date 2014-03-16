/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Threading.Tasks;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Helpers.MusicLibrary;
using VLC_WINRT.Utility.Services.Interface;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT.ViewModels.MainPage.PlayMusic;
using Windows.Media;
using Windows.Storage;

namespace VLC_WINRT.Utility.Services.RunTime
{
    public class MediaService : IMediaService
    {
        private readonly HistoryService _historyService;
        private readonly VlcService _vlcService;

        public MediaService(HistoryService historyService, VlcService vlcService)
        {
            _historyService = historyService;
            _vlcService = vlcService;

            _vlcService.MediaEnded += VlcPlayerService_MediaEnded;
            _vlcService.StatusChanged += VlcPlayerService_StatusChanged;
            MediaControl.IsPlaying = false;
        }

        public Task PlayAudioFile(StorageFile file)
        {
            throw new NotImplementedException();
        }

        public Task PlayVideoFile(StorageFile file)
        {
            throw new NotImplementedException();
        }

        private string lastPath;

        public void SetPath(string filePath)
        {
            _vlcService.Open(filePath);
            lastPath = filePath;
        }

        public void Play()
        {
            var position = GetPosition();
            if (_vlcService.CurrentState == VlcService.MediaPlayerState.NotPlaying && !string.IsNullOrWhiteSpace(lastPath))
            {
                SetPosition(0);
                SetPath(lastPath);
            }
            _vlcService.Play();
            RegisterMediaControls();
        }

        public void Pause()
        {
            _vlcService.Pause();
        }

        public void Stop()
        {
            if (_vlcService.CurrentState == VlcService.MediaPlayerState.Stopped || _vlcService.CurrentState ==  VlcService.MediaPlayerState.NotPlaying)
                return;

            _vlcService.Stop();
            UnregisterMediaControls();
        }

        public void FastForward()
        {
            throw new NotImplementedException();
        }

        public void Rewind()
        {
            throw new NotImplementedException();
        }

        public void SkipAhead()
        {
            _vlcService.SkipAhead();
        }

        public void SkipBack()
        {
            _vlcService.SkipBack();
        }

        public float GetPosition()
        {
            return _vlcService.GetPosition().Result;
        }

        public void SetPosition(float position)
        {
            _vlcService.Seek(position);
        }

        private async void MediaControl_PausePressed(object sender, object e)
        {
            await DispatchHelper.InvokeAsync(() =>
            {
                Pause();
            });
        }

        private async void MediaControl_PlayPressed(object sender, object e)
        {
            await DispatchHelper.InvokeAsync(() =>
            {
                Play();
            });
        }

        void MediaControl_PlayPauseTogglePressed(object sender, object e)
        {
            if (MediaControl.IsPlaying)
                Pause();
            else
                Play();
        }

        async void VlcPlayerService_MediaEnded(object sender, libVLCX.Player e)
        {
            await DispatchHelper.InvokeAsync(() =>
            {
                MediaControl.IsPlaying = false;
                UnregisterMediaControls();

                var mediaEnded = MediaEnded;
                if (mediaEnded != null)
                {
                    mediaEnded(this, new EventArgs());
                }
            });
        }

        private void VlcPlayerService_StatusChanged(object sender, VlcService.MediaPlayerState state)
        {
            MediaControl.IsPlaying = state == VlcService.MediaPlayerState.Playing;
            var statusChanged = StatusChanged;
            if (statusChanged != null)
            {
                statusChanged(this, state);
            }
        }

        private bool _isRegistered;

        private void RegisterMediaControls()
        {
            if (_isRegistered)
                return; 

            MediaControl.PlayPressed += MediaControl_PlayPressed;
            MediaControl.PausePressed += MediaControl_PausePressed;
            MediaControl.PlayPauseTogglePressed += MediaControl_PlayPauseTogglePressed;
            _isRegistered = true;
        }

        private void UnregisterMediaControls()
        {
            if (!_isRegistered)
                return; 

            MediaControl.PlayPressed -= MediaControl_PlayPressed;
            MediaControl.PausePressed -= MediaControl_PausePressed;
            MediaControl.PlayPauseTogglePressed -= MediaControl_PlayPauseTogglePressed;
            _isRegistered = false;
        }

        public bool IsPlaying
        {
            get { return _vlcService.CurrentState == VlcService.MediaPlayerState.Playing; }
        }

        public event EventHandler MediaEnded;
        public event EventHandler<VlcService.MediaPlayerState> StatusChanged;


    }
}
