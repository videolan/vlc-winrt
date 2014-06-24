using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.DataRepository
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
                db.CreateTable<MusicLibraryVM.TrackItem>();

            }
        }

        public
    async Task<MusicLibraryVM.TrackItem> LoadTrack(int artistId, int albumId, string trackName)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<MusicLibraryVM.TrackItem>().Where(x => x.Name.Equals(trackName)).Where(x => x.ArtistId == artistId).Where(x => x.AlbumId == albumId);
            var result = await query.ToListAsync();
            return result.FirstOrDefault();
        }

        public
            async Task<ObservableCollection<MusicLibraryVM.TrackItem>> LoadTracksByAlbumId(int albumId)
        {

            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<MusicLibraryVM.TrackItem>().Where(x => x.AlbumId == albumId);
            var result = await query.ToListAsync();
            return new ObservableCollection<MusicLibraryVM.TrackItem>(result);
        }

        public Task Update(MusicLibraryVM.TrackItem track)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.UpdateAsync(track);
        }

        public Task Add(MusicLibraryVM.TrackItem track)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.InsertAsync(track);
        }
    }
}
