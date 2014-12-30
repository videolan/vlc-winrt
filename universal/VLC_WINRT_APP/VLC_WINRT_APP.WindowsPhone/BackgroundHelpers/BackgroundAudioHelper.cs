using System;
using System.Diagnostics;
using System.Threading;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI.Core;
using VLC_WINRT_APP.Helpers;

namespace VLC_WINRT_APP.BackgroundHelpers
{
    public class BackgroundAudioHelper
    {
        private AutoResetEvent SererInitialized;
        private AutoResetEvent PingEvent;
        private bool isMyBackgroundTaskRunning = false;

        public void InitBackgroundAudio()
        {
            // flecoqui begin
            SererInitialized = new AutoResetEvent(false);
            PingEvent = new AutoResetEvent(false);
            App.Current.Suspending += ForegroundApp_Suspending;
            App.Current.Resuming += ForegroundApp_Resuming;
            ApplicationSettingsHelper.SaveSettingsValue(BackgroundAudioConstants.AppState, BackgroundAudioConstants.ForegroundAppActive);
            //StartAudio();
            // flecoqui end
        }

        /// <summary>
        /// Gets the information about background task is running or not by reading the setting saved by background task
        /// </summary>
        private bool IsMyBackgroundTaskRunning
        {
            get
            {
                if (isMyBackgroundTaskRunning)
                    return true;
                object value = ApplicationSettingsHelper.ReadResetSettingsValue(BackgroundAudioConstants.BackgroundTaskState);
                if (value == null)
                {
                    return false;
                }
                else
                {
                    isMyBackgroundTaskRunning = ((string)value).Equals(BackgroundAudioConstants.BackgroundTaskRunning);
                    return isMyBackgroundTaskRunning;
                }
            }
        }

        /// <summary>
        /// Initialize Background Media Player Handlers and starts playback
        /// </summary>
        private void StartBackgroundAudioTask()
        {
            AddMediaPlayerEventHandlers();

            var backgroundtaskinitializationresult = App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                bool result = SererInitialized.WaitOne(3000);
                //Send message to initiate playback
                if (result == true)
                {
                }
                else
                {
                    throw new Exception("Background Audio Task didn't start in expected time");
                }
            }
            );
            backgroundtaskinitializationresult.Completed = new AsyncActionCompletedHandler(BackgroundTaskInitializationCompleted);
        }

        private void BackgroundTaskInitializationCompleted(IAsyncAction action, AsyncStatus status)
        {
            if (status == AsyncStatus.Completed)
            {
                Debug.WriteLine("Background Audio Task initialized");
            }
            else if (status == AsyncStatus.Error)
            {
                Debug.WriteLine("Background Audio Task could not initialized due to an error ::" + action.ErrorCode.ToString());
            }
        }


        /// <summary>
        /// Unsubscribes to MediaPlayer events. Should run only on suspend
        /// </summary>
        private void RemoveMediaPlayerEventHandlers()
        {
            BackgroundMediaPlayer.Current.CurrentStateChanged -= this.MediaPlayer_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromBackground -= this.BackgroundMediaPlayer_MessageReceivedFromBackground;
        }

        /// <summary>
        /// Subscribes to MediaPlayer events
        /// </summary>
        private void AddMediaPlayerEventHandlers()
        {
            BackgroundMediaPlayer.Current.CurrentStateChanged += this.MediaPlayer_CurrentStateChanged;

            BackgroundMediaPlayer.MessageReceivedFromBackground += this.BackgroundMediaPlayer_MessageReceivedFromBackground;
        }


        /// <summary>
        /// MediaPlayer state changed event handlers. 
        /// Note that we can subscribe to events even if Media Player is playing media in background
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        async void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            switch (sender.CurrentState)
            {
                case MediaPlayerState.Playing:
                    //playButton.Content = "| |";     // Change to pause button
                    //prevButton.IsEnabled = true;
                    //nextButton.IsEnabled = true;
                    Debug.WriteLine("Media State Changed: Playing");

                    break;
                case MediaPlayerState.Paused:
                    //playButton.Content = ">";     // Change to play button
                    Debug.WriteLine("Media State Changed: Paused");

                    break;
            }
        }

        /// <summary>
        /// This event fired when a message is recieved from Background Process
        /// </summary>
        async void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            foreach (string key in e.Data.Keys)
            {
                switch (key)
                {
                    case BackgroundAudioConstants.PingBackground:
                        //When foreground app is active change track based on background message
                        Debug.WriteLine("Ping from background received");
                        PingEvent.Set();
                        break;
                    case BackgroundAudioConstants.Trackchanged:
                        //When foreground app is active change track based on background message
                        Debug.WriteLine("Track Changed" + (string)e.Data[key]);
                        break;
                    case BackgroundAudioConstants.BackgroundTaskStarted:
                        //Wait for Background Task to be initialized before starting playback
                        Debug.WriteLine("Background Task started");
                        SererInitialized.Set();
                        break;
                }
            }
        }

        /// <summary>
        /// Sends message to background informing app has resumed
        /// Subscribe to MediaPlayer events
        /// </summary>
        void ForegroundApp_Resuming(object sender, object e)
        {
            ApplicationSettingsHelper.SaveSettingsValue(BackgroundAudioConstants.AppState, BackgroundAudioConstants.ForegroundAppActive);

            // Verify if the task was running before
            if (IsMyBackgroundTaskRunning)
            {
                //if yes, reconnect to media play handlers
                AddMediaPlayerEventHandlers();

                //send message to background task that app is resumed, so it can start sending notifications
                ValueSet messageDictionary = new ValueSet();
                messageDictionary.Add(BackgroundAudioConstants.AppResumed, DateTime.Now.ToString());
                BackgroundMediaPlayer.SendMessageToBackground(messageDictionary);

                if (BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Playing)
                {
                    //  playButton.Content = "| |";     // Change to pause button
                    Debug.WriteLine("Resuming Media State Playing");
                }
                else
                {
                    // playButton.Content = ">";     // Change to play button
                    Debug.WriteLine("Resuming Media State Not Playing");
                }
                // txtCurrentTrack.Text = CurrentTrack;
            }
            else
            {
                Debug.WriteLine("Resuming Media State Not Playing");
                //playButton.Content = ">";     // Change to play button
                //txtCurrentTrack.Text = "";
            }

        }

        /// <summary>
        /// Send message to Background process that app is to be suspended
        /// Stop clock and slider when suspending
        /// Unsubscribe handlers for MediaPlayer events
        /// </summary>
        void ForegroundApp_Suspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            ValueSet messageDictionary = new ValueSet();
            messageDictionary.Add(BackgroundAudioConstants.AppSuspended, DateTime.Now.ToString());
            BackgroundMediaPlayer.SendMessageToBackground(messageDictionary);
            RemoveMediaPlayerEventHandlers();
            ApplicationSettingsHelper.SaveSettingsValue(BackgroundAudioConstants.AppState, BackgroundAudioConstants.ForegroundAppSuspended);
            deferral.Complete();
        }
    }
}
