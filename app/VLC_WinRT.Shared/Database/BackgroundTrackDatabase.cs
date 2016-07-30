using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VLC_WinRT.Utils;
using VLC_WinRT.Model.Music;
using SQLite;

namespace VLC_WinRT.Database
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
                db.CreateTable<TrackItem>();
            }
        }

        public void Drop()
        {
            using (var db = new SQLiteConnection(DbPath))
            {
                db.DropTable<TrackItem>();
            }
        }

        public void DeleteAll()
        {
            using (var db = new SQLiteConnection(DbPath))
            {
                db.DeleteAll<TrackItem>();
            }
        }

        public async Task Add(TrackItem track)
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

        public async Task Add(IEnumerable<TrackItem> tracks)
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

        public async Task<List<TrackItem>> LoadPlaylist()
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            try
            {
                var connection = new SQLiteAsyncConnection(DbPath);
                var query = connection.Table<TrackItem>().OrderBy(x => x.Name);
                var tracks = await query.ToListAsync();
                return tracks;
            }
            finally
            {
                MusicDatabase.DatabaseOperation.Release();
            }
        }


        public async Task Remove(TrackItem track)
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
            return c.ExecuteAsync("DELETE FROM BackgroundTrackItem");
        }
    }
}
