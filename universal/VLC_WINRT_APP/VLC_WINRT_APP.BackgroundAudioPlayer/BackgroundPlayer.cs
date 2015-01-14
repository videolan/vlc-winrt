using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using VLC_WINRT_APP.BackgroundAudioPlayer.Model;
using VLC_WINRT_APP.Database.DataRepository;

namespace VLC_WINRT_APP.BackgroundAudioPlayer
{
    /// <summary>
    /// Enum to identify foreground app state
    /// </summary>
    enum ForegroundAppStatus
    {
        Active,
        Suspended,
        Unknown
    }

    public sealed class BackgroundPlayer : IBackgroundTask
    {
        #region Private fields, properties
        private SystemMediaTransportControls systemmediatransportcontrol;
        private BackgroundTrackCollection playlistManager;
        private BackgroundTaskDeferral deferral; // Used to keep task alive
        private ForegroundAppStatus foregroundAppState = ForegroundAppStatus.Unknown;
        private AutoResetEvent BackgroundTaskStarted = new AutoResetEvent(false);
        private bool backgroundtaskrunning = false;
        private BackgroundTrackRepository _backgroundTrackRepository = new BackgroundTrackRepository();
        /// <summary>
        /// Property to hold current playlist
        /// </summary>
        private BackgroundTrackCollection Playlist
        {
            get { return playlistManager ?? (playlistManager = new BackgroundTrackCollection()); }
        }
        #endregion

        #region IBackgroundTask and IBackgroundTaskInstance Interface Members and handlers
        /// <summary>
        /// The Run method is the entry point of a background task. 
        /// </summary>
        /// <param name="taskInstance"></param>
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("Background Audio Task " + taskInstance.Task.Name + " starting...");
            // Initialize SMTC object to talk with UVC. 
            //Note that, this is intended to run after app is paused and 
            //hence all the logic must be written to run in background process
            systemmediatransportcontrol = SystemMediaTransportControls.GetForCurrentView();
            systemmediatransportcontrol.ButtonPressed += systemmediatransportcontrol_ButtonPressed;
            systemmediatransportcontrol.PropertyChanged += systemmediatransportcontrol_PropertyChanged;
            systemmediatransportcontrol.IsEnabled = true;
            systemmediatransportcontrol.IsPauseEnabled = true;
            systemmediatransportcontrol.IsPlayEnabled = true;
            systemmediatransportcontrol.IsNextEnabled = true;
            systemmediatransportcontrol.IsPreviousEnabled = true;

            // Associate a cancellation and completed handlers with the background task.
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);
            taskInstance.Task.Completed += Taskcompleted;

            var value = ApplicationSettingsHelper.ReadResetSettingsValue(BackgroundAudioConstants.AppState);
            if (value == null)
                foregroundAppState = ForegroundAppStatus.Unknown;
            else
                foregroundAppState = (ForegroundAppStatus)Enum.Parse(typeof(ForegroundAppStatus), value.ToString());

            //Add handlers for MediaPlayer
            BackgroundMediaPlayer.Current.CurrentStateChanged += Current_CurrentStateChanged;

            //Add handlers for playlist trackchanged
            Playlist.TrackChanged += playList_TrackChanged;

            //Initialize message channel 
            BackgroundMediaPlayer.MessageReceivedFromForeground += BackgroundMediaPlayer_MessageReceivedFromForeground;

            //Send information to foreground that background task has been started if app is active
            if (foregroundAppState != ForegroundAppStatus.Suspended)
            {
                ValueSet message = new ValueSet();
                message.Add(BackgroundAudioConstants.BackgroundTaskStarted, "");
                BackgroundMediaPlayer.SendMessageToForeground(message);
            }
            BackgroundTaskStarted.Set();
            backgroundtaskrunning = true;

            Playlist.PopulatePlaylist();
            var currentTrackIndex = ApplicationSettingsHelper.ReadSettingsValue(BackgroundAudioConstants.CurrentTrack);
            if (currentTrackIndex != null)
            {
                Playlist.CurrentTrack = (int) currentTrackIndex;
            }
            ApplicationSettingsHelper.SaveSettingsValue(BackgroundAudioConstants.BackgroundTaskState, BackgroundAudioConstants.BackgroundTaskRunning);
            deferral = taskInstance.GetDeferral();
        }

        /// <summary>
        /// Indicate that the background task is completed.
        /// </summary>       
        void Taskcompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            Debug.WriteLine("MyBackgroundAudioTask " + sender.TaskId + " Completed...");
            deferral.Complete();
        }

        /// <summary>
        /// Handles background task cancellation. Task cancellation happens due to :
        /// 1. Another Media app comes into foreground and starts playing music 
        /// 2. Resource pressure. Your task is consuming more CPU and memory than allowed.
        /// In either case, save state so that if foreground app resumes it can know where to start.
        /// </summary>
        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            // You get some time here to save your state before process and resources are reclaimed
            Debug.WriteLine("MyBackgroundAudioTask " + sender.Task.TaskId + " Cancel Requested...");
            try
            {
                //save state
                ApplicationSettingsHelper.SaveSettingsValue(BackgroundAudioConstants.CurrentTrack, Playlist.CurrentTrackIndex);
                ApplicationSettingsHelper.SaveSettingsValue(BackgroundAudioConstants.Position, BackgroundMediaPlayer.Current.Position.ToString());
                ApplicationSettingsHelper.SaveSettingsValue(BackgroundAudioConstants.Time, BackgroundMediaPlayer.Current.NaturalDuration.ToString());
                ApplicationSettingsHelper.SaveSettingsValue(BackgroundAudioConstants.BackgroundTaskState, BackgroundAudioConstants.BackgroundTaskCancelled);
                ApplicationSettingsHelper.SaveSettingsValue(BackgroundAudioConstants.AppState, Enum.GetName(typeof(ForegroundAppStatus), foregroundAppState));
                backgroundtaskrunning = false;
                //unsubscribe event handlers
                systemmediatransportcontrol.ButtonPressed -= systemmediatransportcontrol_ButtonPressed;
                systemmediatransportcontrol.PropertyChanged -= systemmediatransportcontrol_PropertyChanged;
                Playlist.TrackChanged -= playList_TrackChanged;

                //clear objects task cancellation can happen uninterrupted
                playlistManager.ResetCollection(ResetType.NormalReset);
                playlistManager = null;

                // Send message to foreground task (if it's around) that this task was cancelled :(
                ValueSet message = new ValueSet { { BackgroundAudioConstants.BackgroundTaskCancelled, "" } };
                BackgroundMediaPlayer.SendMessageToForeground(message);

                BackgroundMediaPlayer.Shutdown(); // shutdown media pipeline
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            deferral.Complete(); // signals task completion. 
            Debug.WriteLine("MyBackgroundAudioTask Cancel complete...");
        }
        #endregion

        #region SysteMediaTransportControls related functions and handlers
        /// <summary>
        /// Update UVC using SystemMediaTransPortControl apis
        /// </summary>
        private void UpdateUVCOnNewTrack()
        {
            systemmediatransportcontrol.PlaybackStatus = MediaPlaybackStatus.Playing;
            systemmediatransportcontrol.DisplayUpdater.Type = MediaPlaybackType.Music;
            systemmediatransportcontrol.DisplayUpdater.MusicProperties.Title = Playlist.CurrentTrackItem.Name;
            systemmediatransportcontrol.DisplayUpdater.MusicProperties.Artist = Playlist.CurrentTrackItem.ArtistName;
            systemmediatransportcontrol.DisplayUpdater.Update();
        }


        /// <summary>
        /// Fires when any SystemMediaTransportControl property is changed by system or user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void systemmediatransportcontrol_PropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            //TODO: If soundlevel turns to muted, app can choose to pause the music
        }

        /// <summary>
        /// This function controls the button events from UVC.
        /// This code if not run in background process, will not be able to handle button pressed events when app is suspended.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void systemmediatransportcontrol_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    Debug.WriteLine("UVC play button pressed");
                    // If music is in paused state, for a period of more than 5 minutes, 
                    //app will get task cancellation and it cannot run code. 
                    //However, user can still play music by pressing play via UVC unless a new app comes in clears UVC.
                    //When this happens, the task gets re-initialized and that is asynchronous and hence the wait
                    if (!backgroundtaskrunning)
                    {
                        bool result = BackgroundTaskStarted.WaitOne(3000);
                        if (!result)
                            throw new Exception("Background Task didnt initialize in time");
                    }
                    StartPlayback();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    Debug.WriteLine("UVC pause button pressed");
                    try
                    {
                        BackgroundMediaPlayer.Current.Pause();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                    break;
                case SystemMediaTransportControlsButton.Next:
                    Debug.WriteLine("UVC next button pressed");
                    SkipToNext();
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    Debug.WriteLine("UVC previous button pressed");
                    SkipToPrevious();
                    break;
            }
        }
        #endregion

        #region Playlist management functions and handlers
        /// <summary>
        /// Start playlist and change UVC state
        /// </summary>

        private void StartPlayback()
        {
            try
            {
                if (!Playlist.IsRunning)
                {
                    Playlist.CurrentTrack = 0;
                    Playlist.Play();
                }
                else
                {
                    BackgroundMediaPlayer.Current.Play();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Fires when playlist changes to a new track
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void playList_TrackChanged(object sender, object args)
        {
            UpdateUVCOnNewTrack();
            ApplicationSettingsHelper.SaveSettingsValue(BackgroundAudioConstants.CurrentTrack, Playlist.CurrentTrackIndex);

            if (foregroundAppState == ForegroundAppStatus.Active)
            {
                //Message channel that can be used to send messages to foreground
                ValueSet message = new ValueSet();
                message.Add(BackgroundAudioConstants.Trackchanged, Playlist.CurrentTrack);
                BackgroundMediaPlayer.SendMessageToForeground(message);
            }
        }

        /// <summary>
        /// Skip track and update UVC via SMTC
        /// </summary>
        private void SkipToPrevious()
        {
            systemmediatransportcontrol.PlaybackStatus = MediaPlaybackStatus.Changing;
            Playlist.SkipToPrevious();
        }

        /// <summary>
        /// Skip track and update UVC via SMTC
        /// </summary>
        private void SkipToNext()
        {
            systemmediatransportcontrol.PlaybackStatus = MediaPlaybackStatus.Changing;
            Playlist.SkipToNext();
        }

        #endregion

        #region Background Media Player Handlers
        void Current_CurrentStateChanged(MediaPlayer sender, object args)
        {
            if (sender.CurrentState == MediaPlayerState.Playing)
            {
                systemmediatransportcontrol.PlaybackStatus = MediaPlaybackStatus.Playing;
            }
            else if (sender.CurrentState == MediaPlayerState.Paused)
            {
                systemmediatransportcontrol.PlaybackStatus = MediaPlaybackStatus.Paused;
            }
        }


        /// <summary>
        /// Fires when a message is recieved from the foreground app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            foreach (string key in e.Data.Keys)
            {
                switch (key.ToLower())
                {
                    case BackgroundAudioConstants.PingBackground:
                        Debug.WriteLine("Received Ping from foreground"); // App is suspended, you can save your task state at this point
                        ValueSet message = new ValueSet();
                        message.Add(BackgroundAudioConstants.PingBackground, DateTime.Now.ToString());
                        BackgroundMediaPlayer.SendMessageToForeground(message);
                        break;
                    case BackgroundAudioConstants.AppSuspended:
                        Debug.WriteLine("App suspending"); // App is suspended, you can save your task state at this point
                        foregroundAppState = ForegroundAppStatus.Suspended;
                        ApplicationSettingsHelper.SaveSettingsValue(BackgroundAudioConstants.CurrentTrack, Playlist.CurrentTrackIndex);
                        break;
                    case BackgroundAudioConstants.AppResumed:
                        Debug.WriteLine("App resuming"); // App is resumed, now subscribe to message channel
                        foregroundAppState = ForegroundAppStatus.Active;
                        break;
                    case BackgroundAudioConstants.StartPlayback: //Foreground App process has signalled that it is ready for playback
                        Debug.WriteLine("Starting Playback");
                        StartPlayback();
                        break;

                    case BackgroundAudioConstants.ResetPlaylist:
                        var arg = new object();
                        if (e.Data.TryGetValue(BackgroundAudioConstants.ResetPlaylist, out arg))
                        {
                            Playlist.ResetCollection((ResetType)arg);
                        }
                        break;
                    case BackgroundAudioConstants.AddTrack:
                        object track = new object();
                        if (e.Data.TryGetValue(BackgroundAudioConstants.AddTrack, out track))
                        {
                            // We should have the newest playlist with the new track already. If so, just repopulate the playlist.
                            Playlist.AddTracks(AudioBackgroundInterface.DeserializeAudioTracks(track.ToString()) as List<BackgroundTrackItem>);
                        }
                        break;
                    case BackgroundAudioConstants.PlayTrack: //Foreground App process has signalled that it is ready for playback
                        Debug.WriteLine("Starting Playback");
                        Object obj = new Object();
                        if (e.Data.TryGetValue(BackgroundAudioConstants.PlayTrack, out obj))
                        {
                            string s = obj as string;
                            if (!string.IsNullOrEmpty(s))
                            {
                                int t = Convert.ToInt32(s);
                                Playlist.PlayTrack(t);
                            }
                        }
                        break;
                    case BackgroundAudioConstants.RestorePlaylist: //Foreground App process updated List Track
                        Debug.WriteLine("Restoring ListTrack");
                        Playlist.PopulatePlaylist();
                        var currentTrack =
                            ApplicationSettingsHelper.ReadSettingsValue(BackgroundAudioConstants.CurrentTrack);
                        if (currentTrack == null)
                        {
                            break;
                        }
                        Playlist.CurrentTrack = (int)currentTrack;
                        break;
                    case BackgroundAudioConstants.SkipNext: // User has chosen to skip track from app context.
                        Debug.WriteLine("Skipping to next");
                        SkipToNext();
                        break;
                    case BackgroundAudioConstants.SkipPrevious: // User has chosen to skip track from app context.
                        Debug.WriteLine("Skipping to previous");
                        SkipToPrevious();
                        break;
                }
            }
        }
        #endregion
    }
}
