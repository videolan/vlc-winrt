using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using VLC_WINRT_APP.Database.Model;

namespace VLC_WINRT_APP.Database.DataRepository
{
    public class BackgroundTrackRepository : IDataRepository
    {
        private static readonly string DbPath =
Path.Combine(
Windows.Storage.ApplicationData.Current.LocalFolder.Path,
"background.sqlite");

        public BackgroundTrackRepository()
        {
            Initialize();
        }

        public void Initialize()
        {
            using (var db = new SQLite.SQLiteConnection(DbPath))
            {
                db.CreateTable<BackgroundTrackItem>();
            }
        }

        public void Drop()
        {
            using (var db = new SQLite.SQLiteConnection(DbPath))
            {
                db.DropTable<BackgroundTrackItem>();
            }
        }

        public void Add(BackgroundTrackItem track)
        {
            var connection = new SQLiteConnection(DbPath);
            connection.Insert(track);
        }

        public async Task<List<BackgroundTrackItem>> LoadPlaylist()
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return new List<BackgroundTrackItem>(await connection.Table<BackgroundTrackItem>().ToListAsync());
        }

        public Task Remove(BackgroundTrackItem track)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.DeleteAsync(track);
        }

        public async Task Clear()
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            await connection.ExecuteAsync("DELETE FROM BackgroundTrackItem");
        }
    }
}
