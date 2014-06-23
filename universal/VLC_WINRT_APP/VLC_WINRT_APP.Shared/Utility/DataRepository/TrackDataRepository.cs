using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT_APP.ViewModels.MainPage;

namespace VLC_WINRT.Utility.DataRepository
{
    public class TrackDataRepository
    {
        private static readonly string _dbPath =
    Path.Combine(
    Windows.Storage.ApplicationData.Current.LocalFolder.Path,
    "mediavlc.sqlite");

        public TrackDataRepository()
        {
            Initialize();
        }

        public void Initialize()
        {
            using (var db = new SQLite.SQLiteConnection(_dbPath))
            {
                db.CreateTable<MusicLibraryViewModel.TrackItem>();

            }
        }

        public
    async Task<MusicLibraryViewModel.TrackItem> LoadTrack(int artistId, int albumId, string trackName)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<MusicLibraryViewModel.TrackItem>().Where(x => x.Name.Equals(trackName)).Where(x => x.ArtistId == artistId).Where(x => x.AlbumId == albumId);
            var result = await query.ToListAsync();
            return result.FirstOrDefault();
        }

        public
            async Task<ObservableCollection<MusicLibraryViewModel.TrackItem>> LoadTracksByAlbumId(int albumId)
        {

            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<MusicLibraryViewModel.TrackItem>().Where(x => x.AlbumId == albumId);
            var result = await query.ToListAsync();
            return new ObservableCollection<MusicLibraryViewModel.TrackItem>(result);
        }

        public Task Update(MusicLibraryViewModel.TrackItem track)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.UpdateAsync(track);
        }

        public Task Add(MusicLibraryViewModel.TrackItem track)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.InsertAsync(track);
        }
    }
}
