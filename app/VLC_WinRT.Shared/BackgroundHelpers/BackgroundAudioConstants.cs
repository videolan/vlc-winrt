namespace VLC_WinRT.BackgroundAudioPlayer.Model
{
    sealed class BackgroundAudioConstants
    {
        public const string CurrentTrack = "trackname";
        public const string BackgroundTaskStarted = "BackgroundTaskStarted";
        public const string BackgroundTaskRunning = "BackgroundTaskRunning";
        public const string BackgroundTaskCancelled = "BackgroundTaskCancelled";
        public const string AppSuspended = "appsuspend";
        public const string AppResumed = "appresumed";
        public const string StartPlayback = "startplayback";
        public const string PlayTrack = "playtrack";
        public const string ListTrack = "listtrack";
        public const string SkipNext = "skipnext";
        public const string Position = "position";
        public const string AppState = "appstate";
        public const string PingBackground = "ping";
        public const string BackgroundTaskState = "backgroundtaskstate";
        public const string SkipPrevious = "skipprevious";
        public const string Trackchanged = "songchanged";
        public const string ForegroundAppActive = "Active";
        public const string ForegroundAppSuspended = "Suspended";

        public const string AddTrack = "addtrack";
        public const string ResetPlaylist = "resetplaylist";
        public const string RestorePlaylist = "restoreplaylist";
        public const string MFFailed = "mediafoundationopenfailed";
        public const string Time = "time";
        public const string ShuffleReset = "shufflereset";
        public const string UpdatePlaylist = "updateplaylist";

        public const string ClearUVC = "clearuvc";
        public const string Repeat = "repeat";
    }
}
