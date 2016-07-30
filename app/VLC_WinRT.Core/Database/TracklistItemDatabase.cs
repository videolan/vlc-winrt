using System.IO;
using System.Threading.Tasks;
using SQLite;
using VLC.Model.Music;
using VLC.ViewModels.MusicVM;
using System.Collections.Generic;
using VLC.Utils;
using VLC.Model;

namespace VLC.Database
{
    public class TracklistItemRepository : IDatabase
    {
        private static readonly string DbPath = Strings.MusicDatabase;

        public TracklistItemRepository()
        {
            Initialize();
        }

        public void Initialize()
        {
            using (var db = new SQLiteConnection(DbPath))
            {
                db.CreateTable<TracklistItem>();
            }
        }

        public void Drop()
        {
            using (var db = new SQLiteConnection(DbPath))
            {
                db.DropTable<TracklistItem>();
            }
        }

        public void DeleteAll()
        {
            using (var db = new SQLiteConnection(DbPath))
            {
                db.DeleteAll<TracklistItem>();
            }
        }

        public Task<List<TracklistItem>> LoadTracks(PlaylistItem trackCollection)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.Table<TracklistItem>().Where(x => x.TrackCollectionId == trackCollection.Id).ToListAsync();
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
