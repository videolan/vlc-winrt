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
using VLC_WINRT_APP.Services.Interface;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.ViewModels;
#if WINDOWS_APP
using libVLCX;
using Windows.Media;
#endif

namespace VLC_WINRT_APP.Services.RunTime
{
#if WINDOWS_PHONE_APP
    public class Player
    {
        public delegate void MediaEndedHandler();

        public Player(SwapChainBackgroundPanel panel)
        {
        }
        public IAsyncAction Initialize()
        {
            return null;
        }

        public void Open(String mrl)
        {
            
        }

        public void Stop()
        {

        }

        public void Pause()
        {

        }
        public void Play()
        {

        }

        public void Seek(float position)
        {

        }

        public float GetPosition()
        {
            return 0f;
        }

        public Int64 GetLength()
        {
            return 0;
        }

        public Int64 GetTime()
        {
            return 0;
        }

        public float GetRate()
        {
            return 0f;
        }

        public int SetRate(float rate)
        {
            return 0;
        }

        public int GetSubtitleCount()
        {
            return 0;
        }

        public int GetSubtitleDescription(IDictionary<int, String> tracks)
        {
            return 0;
        }

        public int SetSubtitleTrack(int track)
        { return 0; }

        public int GetAudioTracksCount()
        { return 0; }

        public int GetAudioTracksDescription(IDictionary<int, String> tracks)
        { return 0; }

        public int SetAudioTrack(int track)
        {
            return 0;
        }

        public int SetVolume(int volume)
        { return 0; }

        public int GetVolume()
        { return 0; }

        public void DetachEvent()
        {

        }
        public void UpdateSize(uint x, uint y)
        {

        }

        public void OpenSubtitle(String mrl)
        {

        }

        public event MediaEndedHandler MediaEnded;

        public void Dispose()
        {

        }
    }
#endif
    public class MediaService : IMediaService
    {
        private readonly VlcService _vlcService;

        private MediaElement _mediaElement;

        public MediaService(VlcService vlcService)
        {
            _vlcService = vlcService;

            _vlcService.MediaEnded += VlcPlayerService_MediaEnded;
            _vlcService.StatusChanged += VlcPlayerService_StatusChanged;
            MediaControl.IsPlaying = false;

            CoreWindow.GetForCurrentThread().Activated += ApplicationState_Activated;
        }

        public void SetMediaElement(MediaElement mediaElement)
        {
            _mediaElement = mediaElement;
        }

        public Task PlayAudioFile(StorageFile file)
        {
            throw new NotImplementedException();
        }

        public Task PlayVideoFile(StorageFile file)
        {
            throw new NotImplementedException();
        }

        private string _lastMrl;
        private bool _isAudioMedia;

        public void SetMediaFile(string filePath, bool isAudioMedia = true)
        {
            _vlcService.Open(filePath);
            _isAudioMedia = isAudioMedia;
            _lastMrl = filePath;
        }

        public void Play()
        {
            var position = GetPosition();
            if (_vlcService.CurrentState == VlcService.MediaPlayerState.NotPlaying && !string.IsNullOrWhiteSpace(_lastMrl))
            {
                SetPosition(0);
                SetMediaFile(_lastMrl, _isAudioMedia);
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
            if (_vlcService.CurrentState == VlcService.MediaPlayerState.Stopped || _vlcService.CurrentState == VlcService.MediaPlayerState.NotPlaying)
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

        async void VlcPlayerService_MediaEnded(object sender, Player e)
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

        private async void ApplicationState_Activated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == CoreWindowActivationState.Deactivated)
            {
                IsBackground = true;
                if (!IsPlaying)
                    return;

                // If we're playing a video, just pause.
                if (!_isAudioMedia)
                {
                    // TODO: Route Video Player calls through Media Service
                    _vlcService.Pause();

                    Locator.VideoVm._lastVideosRepository.Update(Locator.VideoVm.CurrentVideo);
                    return;
                }

                // Otherwise, set the MediaElement's source to the Audio File in question,
                // and play it with a volume of zero. This allows _vlcService's audio to continue
                // to play in the background. SetSource should have it's source set to a programmatically
                // generated stream of blank noise, just incase the audio file in question isn't support by
                // Windows.
                // TODO: generate blank wave file
                //var media = await Locator.PlayVideoVM._lastVideosRepository.LoadViaToken(_lastMrl.Replace("file://", ""));
                //var stream = await media.File.OpenAsync(FileAccessMode.Read);
                //_mediaElement.SetSource(stream, media.File.ContentType);
                //_mediaElement.Play();
                //_mediaElement.IsLooping = true;
                //_mediaElement.Volume = 0;
            }
            else
            {
                IsBackground = false;    

                if (!IsPlaying)
                    return;

                // If we're playing a video, start playing again.
                if (!_isAudioMedia && IsPlaying)
                {
                    // TODO: Route Video Player calls through Media Service
                    _vlcService.Play();
                    return;
                }


                _mediaElement.Stop();
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

        public bool IsBackground { get; private set; }

        public event EventHandler MediaEnded;
        public event EventHandler<VlcService.MediaPlayerState> StatusChanged;


    }
}
