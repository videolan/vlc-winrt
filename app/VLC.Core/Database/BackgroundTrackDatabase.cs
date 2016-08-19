using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VLC.Utils;
using VLC.Model.Music;
using SQLite;

namespace VLC.Database
{
    public class BackgroundTrackDatabase
    {
        private static readonly string DbPath = Strings.MusicBackgroundDatabase;

        public BackgroundTrackDatabase()
        {
            Initialize();
        }

        public void Initialize()
        {
            using (var db = new SQLiteConnection(DbPath))
            {
                db.CreateTable<BackgroundTrackItem>();
            }
        }

        public void Drop()
        {
            using (var db = new SQLiteConnection(DbPath))
            {
                db.DropTable<BackgroundTrackItem>();
            }
        }

        public void DeleteAll()
        {
            using (var db = new SQLiteConnection(DbPath))
            {
                db.DeleteAll<BackgroundTrackItem>();
            }
        }

        public async Task Add(BackgroundTrackItem track)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            try
            {
                var connection = new SQLiteAsyncConnection(DbPath);
                await connection.InsertAsync(track);
            }
            finally
            {
                MusicDatabase.DatabaseOperation.Release();
            }
        }

        public async Task Add(IEnumerable<BackgroundTrackItem> tracks)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            try
            {
                var connection = new SQLiteAsyncConnection(DbPath);
                await connection.InsertAllAsync(tracks);
            }
            finally
            {
                MusicDatabase.DatabaseOperation.Release();
            }
        }

        public async Task<List<BackgroundTrackItem>> LoadPlaylist()
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            try
            {
                var connection = new SQLiteAsyncConnection(DbPath);
                var query = connection.Table<BackgroundTrackItem>();
                var tracks = await query.ToListAsync();
                return tracks;
            }
            finally
            {
                MusicDatabase.DatabaseOperation.Release();
            }
        }


        public async Task Remove(BackgroundTrackItem track)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            try
            {
                var connection = new SQLiteAsyncConnection(DbPath);
                await connection.DeleteAsync(track);
            }
            finally
            {
                MusicDatabase.DatabaseOperation.Release();
            }
        }

        public Task Clear()
        {
            var c = new SQLiteAsyncConnection(DbPath);
            return c.ExecuteAsync($"DELETE FROM {nameof(BackgroundTrackItem)}");
        }
    }
}
