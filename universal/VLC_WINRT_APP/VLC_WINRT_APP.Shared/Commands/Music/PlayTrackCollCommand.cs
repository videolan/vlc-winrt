using VLC_WINRT.Common;
using VLC_WINRT_APP.Helpers.MusicPlayer;
using VLC_WINRT_APP.ViewModels.MusicVM;
using VLC_WINRT_APP.Views.MusicPages;

namespace VLC_WINRT_APP.Commands.Music
{
    public class PlayTrackCollCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            TrackCollection trackCollection = null;
            if (parameter is TrackCollection)
            {
                trackCollection = parameter as TrackCollection;
            }
            await PlayMusicHelper.AddTrackCollectionToPlaylist(trackCollection);
            if (App.ApplicationFrame.CurrentSourcePageType != typeof (MusicPlayerPage))
                App.ApplicationFrame.Navigate(typeof (MusicPlayerPage));
        }
    }
}
