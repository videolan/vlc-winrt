using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage;

namespace VLC_WINRT_APP.BackgroundAudioPlayer
{
    public sealed class BackgroundTrackCollection
    {
        private SystemMediaTransportControls systemmediatransportcontrol;
        #region public properties
        public int Id { get; set; }

        public string Name { get; set; }

        public int CurrentTrack { get; set; }
        public BackgroundTrackItem CurrentTrackId { get; set; }

        public bool CanGoPrevious
        {
            get
            {
                return (CurrentTrack > 0);
            }
        }

        public bool CanGoNext
        {
            get { return (Playlist.Count != 1) && (CurrentTrack < Playlist.Count - 1); }
        }

        public bool IsRunning { get; set; }

        public bool IsShuffled { get; set; }
        #endregion
        #region public fields

        static List<BackgroundTrackItem> Playlist
        {
            get; 
            set;
        }
        #endregion
        #region events
        /// <summary>
        /// Invoked when the media player is ready to move to next track
        /// </summary>
        public event TypedEventHandler<object, object> TrackChanged;
        #endregion
        #region private objects
        private MediaPlayer mediaPlayer;
        #endregion

        #region ctors
        public BackgroundTrackCollection()
        {
            Playlist = new List<BackgroundTrackItem>();
            mediaPlayer = BackgroundMediaPlayer.Current;
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            mediaPlayer.MediaEnded += MediaPlayerOnMediaEnded;
            mediaPlayer.CurrentStateChanged += MediaPlayerOnCurrentStateChanged;
            mediaPlayer.MediaFailed += MediaPlayerOnMediaFailed;
            ResetCollection();
        }

        private void MediaPlayerOnMediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            Debug.WriteLine("Failed to open the file with Background Media Player");
            if (CurrentTrackId != null)
                BackgroundMediaPlayer.SendMessageToForeground(new ValueSet()
                {
                    new KeyValuePair<string, object>(BackgroundAudioConstants.MFFailed, CurrentTrackId.Id)
                });
        }

        private void MediaPlayerOnCurrentStateChanged(MediaPlayer sender, object args)
        {

        }

        private void MediaPlayerOnMediaEnded(MediaPlayer sender, object args)
        {
            if (CanGoNext)
            {
                SkipToNext();
            }
        }

        /// <summary>
        /// Fired when MediaPlayer is ready to play the track
        /// </summary>
        void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            // wait for media to be ready
            sender.Play();
            TrackChanged.Invoke(this, null);
        }
        #endregion

        #region methods

        public void ResetCollection()
        {
            Playlist.Clear();
            CurrentTrack = -1;
        }

        public void SetActiveTrackProperty()
        {
            foreach (var BackgroundTrackItem in Playlist)
            {
                if (Playlist[CurrentTrack].Id == BackgroundTrackItem.Id)
                {
                    BackgroundTrackItem.IsCurrentPlaying = true;
                }
                else
                {
                    BackgroundTrackItem.IsCurrentPlaying = false;
                }
            }
        }

        public void SkipToPrevious()
        {
            if (!CanGoPrevious) return;
            CurrentTrack--;
            Play();
        }

        public void SkipToNext()
        {
            if (!CanGoNext) return;
            CurrentTrack++;
            Play();
        }

        public async void PlayTrack(BackgroundTrackItem track)
        {
            var trackCol = Playlist.FirstOrDefault(x => x.Id == track.Id);
            if (trackCol != null)
            {
                CurrentTrack = Playlist.IndexOf(trackCol);
                Play();
            }
        }

        public async void Play()
        {
            IsRunning = true;
            CurrentTrackId = Playlist[CurrentTrack];
            var file = await StorageFile.GetFileFromPathAsync(Playlist[CurrentTrack].Path);
            mediaPlayer.SetFileSource(file);

            systemmediatransportcontrol = SystemMediaTransportControls.GetForCurrentView();
            systemmediatransportcontrol.IsNextEnabled = CanGoNext;
            systemmediatransportcontrol.IsPreviousEnabled = CanGoPrevious;
        }

        public void AddTrack(BackgroundTrackItem trackItem)
        {
            Playlist.Add(trackItem);
        }
        #endregion
    }
}
