using System.Linq;
using System.Threading.Tasks;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.Helpers
{
    public static class PlayMusickHelper
    {
        public static async Task Play(this MusicLibraryVM.TrackItem track)
        {
            if (track != null && !Locator.MusicPlayerVM.TrackCollection.Contains(track))
            {
                Locator.MusicPlayerVM.ResetCollection();
                Locator.MusicPlayerVM.AddTrack(track);
            }
            else
            {
                Locator.MusicPlayerVM.CurrentTrack =
                    Locator.MusicPlayerVM.TrackCollection.IndexOf(track);
            }
            await Locator.MusicPlayerVM.Play();
        }

        public static async Task Play(this MusicLibraryVM.AlbumItem album)
        {
            Locator.MusicPlayerVM.ResetCollection();
            Locator.MusicPlayerVM.AddTrack(album.Tracks.ToList());
            await Locator.MusicPlayerVM.Play();
        }
    }
}
