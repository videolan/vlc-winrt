using System.Collections.Generic;
using System.Linq;
using VLC.Utils;
using VLC.Model.Music;
using SQLite;

namespace VLC.Database
{
    public class BackgroundTrackDatabase
    {
        private static readonly string DbPath = Strings.MusicBackgroundDatabase;

        private SQLiteConnectionWithLock connection = new SQLiteConnectionWithLock(new SQLiteConnectionString(DbPath, false),
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex | SQLiteOpenFlags.SharedCache);

        public BackgroundTrackDatabase()
        {
            Initialize();
        }

        public void Initialize()
        {
            using (connection.Lock())
            {
                connection.CreateTable<BackgroundTrackItem>();
            }
        }

        public void Drop()
        {
            using (connection.Lock())
            {
                connection.DropTable<BackgroundTrackItem>();
            }
        }

        public void DeleteAll()
        {
            using (connection.Lock())
            {
                connection.DeleteAll<BackgroundTrackItem>();
            }
        }

        public void Add(BackgroundTrackItem track)
        {
            using (connection.Lock())
            {
                connection.Insert(track);
            }
        }

        public void Add(IEnumerable<BackgroundTrackItem> tracks)
        {
            using (connection.Lock())
            {
                connection.InsertAll(tracks);
            }
        }

        public List<BackgroundTrackItem> LoadPlaylist()
        {
            using (connection.Lock())
            {
                return connection.Table<BackgroundTrackItem>().ToList();
            }
        }

        public void Remove(BackgroundTrackItem track)
        {
            using (connection.Lock())
            {
                connection.Delete(track);
            }
        }

        public void Clear()
        {
            using (connection.Lock())
            {
                connection.Execute($"DELETE FROM {nameof(BackgroundTrackItem)}");
            }
        }
    }
}
