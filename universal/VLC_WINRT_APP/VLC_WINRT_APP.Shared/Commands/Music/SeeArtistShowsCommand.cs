using VLC_WINRT.Common;
using VLC_WINRT_APP.Views.MusicPages.ArtistPages;

namespace VLC_WINRT_APP.Commands.Music
{
    public class SeeArtistShowsCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            App.ApplicationFrame.Navigate(typeof (ArtistShowsPage));
        }
    }
}
