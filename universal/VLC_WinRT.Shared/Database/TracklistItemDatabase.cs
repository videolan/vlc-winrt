using System.IO;
using System.Threading.Tasks;
using SQLite;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels.MusicVM;
using System.Collections.Generic;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Database
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

        public Task<List<TracklistItem>> LoadTracks(TrackCollection trackCollection)
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
