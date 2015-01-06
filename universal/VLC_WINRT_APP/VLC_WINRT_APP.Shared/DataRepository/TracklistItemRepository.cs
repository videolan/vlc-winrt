using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using SQLite;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.DataRepository
{
    public class TracklistItemRepository : IDataRepository
    {
        private static readonly string DbPath =
   Path.Combine(
   Windows.Storage.ApplicationData.Current.LocalFolder.Path,
   "mediavlc.sqlite");

        public TracklistItemRepository()
        {
            Initialize();
        }

        public void Initialize()
        {
            using (var db = new SQLite.SQLiteConnection(DbPath))
            {
                db.CreateTable<TracklistItem>();
            }
        }

        public void Drop()
        {
            using (var db = new SQLite.SQLiteConnection(DbPath))
            {
                db.DropTable<TracklistItem>();
            }
        }

        public async Task<ObservableCollection<TracklistItem>> LoadTracks(TrackCollection trackCollection)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return new ObservableCollection<TracklistItem>(await connection.Table<TracklistItem>().Where(x => x.TrackCollectionId == trackCollection.Id).ToListAsync());
        }

        public Task Add(TracklistItem track)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.InsertAsync(track);
        }

        public Task Remove(TracklistItem track)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.DeleteAsync(track);
        }

        public Task Remove(int trackId, int trackCollectionId)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.ExecuteAsync("DELETE FROM TracklistItem WHERE TrackCollectionId=? AND TrackId=?;", trackCollectionId, trackId);
        }

        public async Task Clear()
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            await connection.ExecuteAsync("DELETE FROM TracklistItem");
        }
    }
}
