using SQLite;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VLC_WinRT.BackgroundAudioPlayer.Model;

namespace VLC_WinRT.SharedBackground.Database
{
    public class BackgroundTrackDatabase
    {
        private static readonly string DbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "background.sqlite");

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

        public Task Add(BackgroundTrackItem track)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.InsertAsync(track);
        }

        public Task AddBunchTracks(dynamic tracks)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.InsertAllAsync(tracks);
        }

        public List<BackgroundTrackItem> LoadPlaylist()
        {
            var connection = new SQLiteConnection(DbPath);
            var list = new List<BackgroundTrackItem>(connection.Table<BackgroundTrackItem>().ToList());
            return list;
        }


        public Task Remove(BackgroundTrackItem track)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.DeleteAsync(track);
        }

        public void Clear()
        {
            var connection = new SQLiteConnection(DbPath);
            connection.Execute("DELETE FROM BackgroundTrackItem");
        }
    }
}
