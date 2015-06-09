using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using SQLite.Net.Async;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VLC_WinRT.BackgroundAudioPlayer.Model;

namespace VLC_WinRT.SharedBackground.Database
{
    public class BackgroundTrackDatabase
    {
        private SQLitePlatformWinRT platform = new SQLitePlatformWinRT();
        private SQLiteConnectionWithLock connectionLock;
        private static readonly string DbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "background.sqlite");

        public BackgroundTrackDatabase()
        {
            Initialize();
        }

        public void Initialize()
        {
            connectionLock = new SQLiteConnectionWithLock(platform, new SQLiteConnectionString(DbPath, false));
            var connection = new SQLiteAsyncConnection(() => connectionLock);
            connection.CreateTableAsync<BackgroundTrackItem>();
        }

        public void Drop()
        {
            connectionLock = new SQLiteConnectionWithLock(platform, new SQLiteConnectionString(DbPath, false));
            var connection = new SQLiteAsyncConnection(() => connectionLock);
            connection.DropTableAsync<BackgroundTrackItem>();
        }

        public Task Add(BackgroundTrackItem track)
        {
            connectionLock = new SQLiteConnectionWithLock(platform, new SQLiteConnectionString(DbPath, false));
            var connection = new SQLiteAsyncConnection(() => connectionLock);
            return connection.InsertAsync(track);
        }

        public Task AddBunchTracks(dynamic tracks)
        {
            connectionLock = new SQLiteConnectionWithLock(platform, new SQLiteConnectionString(DbPath, false));
            var connection = new SQLiteAsyncConnection(() => connectionLock);
            return connection.InsertAllAsync(tracks);
        }

        public List<BackgroundTrackItem> LoadPlaylist()
        {
            using (var c = new SQLiteConnection(platform, DbPath))
            {
                var list = new List<BackgroundTrackItem>(c.Table<BackgroundTrackItem>().ToList());
                return list;
            }
        }


        public Task Remove(BackgroundTrackItem track)
        {
            connectionLock = new SQLiteConnectionWithLock(platform, new SQLiteConnectionString(DbPath, false));
            var connection = new SQLiteAsyncConnection(() => connectionLock);
            return connection.DeleteAsync(track);
        }

        public void Clear()
        {
            using (var c = new SQLiteConnection(platform, DbPath))
            {
                c.Execute("DELETE FROM BackgroundTrackItem");
            }
        }
    }
}
