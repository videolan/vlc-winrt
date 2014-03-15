/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Media;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using VLC_WINRT.Utility.Commands;
using VLC_WINRT.Utility.Commands.MusicPlayer;
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.Utility.IoC;
using VLC_WINRT.Utility.Services.RunTime;

namespace VLC_WINRT.ViewModels.MainPage.PlayMusic
{
    public class MusicPlayerViewModel : NavigateableViewModel, IDisposable
    {
        private readonly DisplayRequest _displayAlwaysOnRequest;
        private readonly HistoryService _historyService;
        private readonly DispatcherTimer _sliderPositionTimer = new DispatcherTimer();
        private TimeSpan _elapsedTime = TimeSpan.Zero;
        private string _fileToken;
        private StopVideoCommand _goBackCommand;
        private bool _isPlaying;
        private string _mrl;
        private MusicPlayOrPauseCommand _playOrPause;
        private ActionCommand _skipAhead;
        private ActionCommand _skipBack;
        private PlayNextCommand _playNext;
        private PlayPreviousCommand _playPrevious;
        private TimeSpan _timeTotal = TimeSpan.Zero;
        private string _title;
        private MusicLibraryViewModel.ArtistItemViewModel _artist;
        private MediaPlayerService _vlcPlayerService;
        private TrackCollectionViewModel _trackCollection;
        public MusicPlayerViewModel()
        {
            _playOrPause = new MusicPlayOrPauseCommand();
            _historyService = IoC.GetInstance<HistoryService>();
            _goBackCommand = new StopVideoCommand();
            _displayAlwaysOnRequest = new DisplayRequest();

            _sliderPositionTimer.Tick += FirePositionUpdate;
            _sliderPositionTimer.Interval = TimeSpan.FromMilliseconds(16);

            _vlcPlayerService = IoC.GetInstance<MediaPlayerService>();
            _vlcPlayerService.StatusChanged += PlayerStateChanged;
            _vlcPlayerService.MediaEnded += _vlcPlayerService_MediaEnded;
            _skipAhead = new ActionCommand(() => _vlcPlayerService.SkipAhead());
            _skipBack = new ActionCommand(() => _vlcPlayerService.SkipBack());
            _playNext = new PlayNextCommand();
            _playPrevious = new PlayPreviousCommand();
            _trackCollection = new TrackCollectionViewModel();
        }

        void RegisterWindows8Events()
        {
            // register the windows 8 overlay media control events
            MediaControl.StopPressed += MediaControl_StopPressed;
            MediaControl.PlayPressed += MediaControl_PlayPressed;
            MediaControl.PausePressed += MediaControl_PausePressed;
            MediaControl.IsPlaying = false;
        }

        void UnRegisterWindows8Events()
        {
            MediaControl.StopPressed -= MediaControl_StopPressed;
            MediaControl.PlayPressed -= MediaControl_PlayPressed;
            MediaControl.PausePressed -= MediaControl_PausePressed;
            MediaControl.IsPlaying = false;
        }

        void MediaControl_PreviousTrackPressed(object sender, object e)
        {
            App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, PlayPrevious);
        }

        void MediaControl_NextTrackPressed(object sender, object e)
        {
            App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, PlayNext);
        }
        private void MediaControl_PausePressed(object sender, object e)
        {
            App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, Pause);
        }

        private void MediaControl_PlayPressed(object sender, object e)
        {
            App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _vlcPlayerService.Play();
                MediaControl.IsPlaying = true;
            });
        }

        private void MediaControl_StopPressed(object sender, object e)
        {
            App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _vlcPlayerService.Stop();
                MediaControl.IsPlaying = false;
            });
        }

        public TrackCollectionViewModel TrackCollection
        {
            get
            {
                return _trackCollection;
            }
        }

        void _vlcPlayerService_MediaEnded(object sender, libVLCX.Player e)
        {
            App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (IsPlaying && TrackCollection.TrackCollection != null && TrackCollection.TrackCollection.Any())
                {
                    if (TrackCollection.TrackCollection[TrackCollection.CurrentTrack] ==
                        TrackCollection.TrackCollection.Last())
                    {
                        // Playlist is finished
                        TrackCollection.IsRunning = false;
                        UnRegisterWindows8Events();
                    }
                    else
                    {
                        PlayNext();
                    }
                }
            });
        }

        public void Resume()
        {
            _vlcPlayerService.Play();
            MediaControl.IsPlaying = true;
        }
        public void Stop()
        {
            if (MediaControl.IsPlaying)
                _vlcPlayerService.Stop();
        }
        public void PlayNext()
        {
            TrackCollection.IsNextPossible();
            TrackCollection.IsPreviousPossible();
            if (TrackCollection.CanGoNext)
            {
                TrackCollection.CurrentTrack++;
                Play();
            }
            else
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
                MediaControl.IsPlaying = false; 
                UnRegisterWindows8Events();
            }
        }

        public void PlayPrevious()
        {
            TrackCollection.IsNextPossible();
            TrackCollection.IsPreviousPossible();
            if (TrackCollection.CanGoPrevious)
            {
                TrackCollection.CurrentTrack--;
                Play();
            }
            else
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
                MediaControl.IsPlaying = false;
                MediaControl.PreviousTrackPressed -= MediaControl_PreviousTrackPressed;
                UnRegisterWindows8Events();
            }
        }

        public async void Play()
        {
            TrackCollection.IsRunning = true;
            Stop();
            var trackItem = TrackCollection.TrackCollection[TrackCollection.CurrentTrack];

            var file = await StorageFile.GetFileFromPathAsync(trackItem.Path);
            var history = new HistoryService();
            //string token = history.Add(file, true);
            string token = history.Add(file);

            Debug.WriteLine("Opening file: " + file.Path);

            SetActiveMusicInfo(token, trackItem);

            // Setting the info for windows 8 controls
            var resourceLoader = new ResourceLoader();
            MediaControl.IsPlaying = true;
            MediaControl.ArtistName = trackItem.ArtistName ?? resourceLoader.GetString("UnknownArtist");
            MediaControl.TrackName = trackItem.Name ?? resourceLoader.GetString("UnknownTrack");
            _timeTotal = TimeSpan.Zero;
            _elapsedTime = TimeSpan.Zero;
            try
            {
                MediaControl.AlbumArt = new Uri(Locator.MusicPlayerVM.Artist.CurrentAlbumItem.Picture);
            }
            catch
            {
                // If album cover is from the internet then it's impossible to pass it to the MediaControl
            }

            TrackCollection.IsNextPossible();
            TrackCollection.IsPreviousPossible();

            if (TrackCollection.CanGoNext)
                MediaControl.NextTrackPressed += MediaControl_NextTrackPressed;
            else
                MediaControl.NextTrackPressed -= MediaControl_NextTrackPressed;

            if (TrackCollection.CanGoPrevious)
                MediaControl.PreviousTrackPressed += MediaControl_PreviousTrackPressed;
            else
                MediaControl.PreviousTrackPressed -= MediaControl_PreviousTrackPressed;
        }

        public async void PlayFromExplorer(StorageFile file)
        {
            TrackCollection.IsRunning = true;
            Stop();
            var trackItem = TrackCollection.TrackCollection[TrackCollection.CurrentTrack];

            var history = new HistoryService();
            string token = history.Add(file);

            Debug.WriteLine("Opening file: " + file.Path);

            SetActiveMusicInfo(token, trackItem);

            // Setting the info for windows 8 controls
            MediaControl.IsPlaying = true;
            MediaControl.ArtistName = trackItem.ArtistName;
            MediaControl.TrackName = trackItem.Name;
            try
            {
                MediaControl.AlbumArt = new Uri(Locator.MusicPlayerVM.Artist.CurrentAlbumItem.Picture);
            }
            catch
            {
                // If album cover is from the internet then it's impossible to pass it to the MediaControl
            }

            TrackCollection.IsNextPossible();
            TrackCollection.IsPreviousPossible();

            if (TrackCollection.CanGoNext)
                MediaControl.NextTrackPressed += MediaControl_NextTrackPressed;
            else
                MediaControl.NextTrackPressed -= MediaControl_NextTrackPressed;

            if (TrackCollection.CanGoPrevious)
                MediaControl.PreviousTrackPressed += MediaControl_PreviousTrackPressed;
            else
                MediaControl.PreviousTrackPressed -= MediaControl_PreviousTrackPressed;
        }
        public void Pause()
        {
            _vlcPlayerService.Pause();
            MediaControl.IsPlaying = false;
        }

        public double PositionInSeconds
        {
            get
            {
                if (_vlcPlayerService != null && _vlcPlayerService.CurrentState == MediaPlayerService.MediaPlayerState.Playing)
                {
                    return _vlcPlayerService.GetPosition().Result * TimeTotal.TotalSeconds;
                }
                return 0.0d;
            }
            set { _vlcPlayerService.Seek((float)(value / TimeTotal.TotalSeconds)); }
        }

        public double Position
        {
            get
            {
                if (_vlcPlayerService != null && _vlcPlayerService.CurrentState == MediaPlayerService.MediaPlayerState.Playing)
                {
                    return _vlcPlayerService.GetPosition().Result * 1000;
                }
                return 0.0d;
                
            }
            set
            {
                _vlcPlayerService.Seek((float)value / 1000);
            }
        }

        public string Now
        {
            get { return DateTime.Now.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern); }
        }

        public MusicPlayOrPauseCommand PlayOrPause
        {
            get { return _playOrPause; }
            set { SetProperty(ref _playOrPause, value); }
        }
        public PlayNextCommand PlayNextCommand
        {
            get { return _playNext; }
            set { SetProperty(ref _playNext, value); }
        }
        public PlayPreviousCommand PlayPreviousCommand
        {
            get { return _playPrevious; }
            set { SetProperty(ref _playPrevious, value); }
        }
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                SetProperty(ref _isPlaying, value);
                if (value)
                {
                    _sliderPositionTimer.Start();
                    ProtectedDisplayCall(true);
                }
                else
                {
                    _sliderPositionTimer.Stop();
                    ProtectedDisplayCall(false);
                }
            }
        }

        public ActionCommand SkipAhead
        {
            get { return _skipAhead; }
            set { SetProperty(ref _skipAhead, value); }
        }

        public ActionCommand SkipBack
        {
            get { return _skipBack; }
            set { SetProperty(ref _skipBack, value); }
        }

        public string Title
        {
            get { return _title; }
            private set { SetProperty(ref _title, value); }
        }

        public MusicLibraryViewModel.ArtistItemViewModel Artist
        {
            get { return _artist; }
            private set { SetProperty(ref _artist, value); }
        }

        public StopVideoCommand GoBack
        {
            get { return _goBackCommand; }
            set { SetProperty(ref _goBackCommand, value); }
        }


        public TimeSpan TimeTotal
        {
            get { return _timeTotal; }
            set { SetProperty(ref _timeTotal, value); }
        }

        public TimeSpan ElapsedTime
        {
            get { return _elapsedTime; }
            set { SetProperty(ref _elapsedTime, value); }
        }

        public void Dispose()
        {
            if (_vlcPlayerService != null)
            {
                _vlcPlayerService.StatusChanged -= PlayerStateChanged;
                _vlcPlayerService.Stop();
                _vlcPlayerService.Close();
                _vlcPlayerService = null;
            }

            _skipAhead = null;
            _skipBack = null;
        }

        private void FirePositionUpdate(object sender, object e)
        {
            UpdatePosition(this, e);
        }

        private void PlayerStateChanged(object sender, MediaPlayerService.MediaPlayerState e)
        {
            if (e == MediaPlayerService.MediaPlayerState.Playing)
            {
                IsPlaying = true;
            }
            else
            {
                IsPlaying = false;
            }
        }

        public void RegisterPanel()
        {

        }

        public void SetActiveMusicInfo(string token, MusicLibraryViewModel.TrackItem track)
        {
            _fileToken = token;
            _mrl = "file://" + token;
            Title = track.Name;
            Artist = Locator.MusicLibraryVM.Artist.FirstOrDefault(x => x.Name == track.ArtistName);
            if (Artist != null)
                Artist.CurrentAlbumIndex = _artist.Albums.IndexOf(_artist.Albums.FirstOrDefault(x => x.Name == track.AlbumName));
            _vlcPlayerService.Open(_mrl);
            OnPropertyChanged("TimeTotal");
            UpdateTileHelper.UpdateMediumTileWithMusicInfo();
            _vlcPlayerService.Play();

            RegisterWindows8Events();
        }

        public override Task OnNavigatedFrom()
        {
            _sliderPositionTimer.Stop();
            _vlcPlayerService.Stop();
            return base.OnNavigatedFrom();
        }

        private void UpdateDate(object sender, object e)
        {
            if (!string.IsNullOrEmpty(_fileToken))
            {
                _historyService.UpdateMediaHistory(_fileToken, ElapsedTime);
            }
            OnPropertyChanged("Now");
        }

        private void ProtectedDisplayCall(bool shouldActivate)
        {
            if (_displayAlwaysOnRequest == null) return;
            try
            {
                if (shouldActivate)
                {
                    _displayAlwaysOnRequest.RequestActive();
                }
                else
                {
                    _displayAlwaysOnRequest.RequestRelease();
                }
            }
            catch (ArithmeticException badMathEx)
            {
                //  Work around for platform bug 
                Debug.WriteLine("display request failed again");
                Debug.WriteLine(badMathEx.ToString());
            }
        }

        private async Task UpdatePosition(object sender, object e)
        {
            OnPropertyChanged("PositionInSeconds");

            if (_timeTotal == TimeSpan.Zero)
            {
                double timeInMilliseconds = await _vlcPlayerService.GetLength();
                TimeTotal = TimeSpan.FromMilliseconds(timeInMilliseconds);
            }

            OnPropertyChanged("Position");
            ElapsedTime = TimeSpan.FromSeconds(PositionInSeconds);
        }

        public async Task CleanViewModel()
        {
            _vlcPlayerService.Stop();
            TrackCollection.TrackCollection.Clear();
            TrackCollection.IsRunning = false;
            IsPlaying = false;
            Pause();
            Artist = null;
            Title = null;
            _elapsedTime = TimeSpan.Zero;
            _timeTotal = TimeSpan.Zero;
            MediaControl.IsPlaying = false;
        }
    }
}
