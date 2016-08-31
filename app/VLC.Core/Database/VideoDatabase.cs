using SQLite;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VLC.Model.Video;
using VLC.Utils;

namespace VLC.Database
{
    public class VideoRepository : IDatabase
    {
        private static readonly string DbPath = Strings.VideoDatabase;
        private SQLiteConnectionWithLock _connection;

        private SQLiteConnectionWithLock Connection => _connection ?? (_connection = new SQLiteConnectionWithLock(new SQLiteConnectionString(DbPath, false),
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex | SQLiteOpenFlags.SharedCache));

        public VideoRepository()
        {
            Initialize();
        }

        public void Initialize()
        {
            using (Connection.Lock())
            {
                Connection.CreateTable<VideoItem>();
            }
        }

        public void Drop()
        {
            using (Connection.Lock())
            {
                Connection.DropTable<VideoItem>();
            }
        }

        public void DeleteAll()
        {
            using (Connection.Lock())
            {
                Connection.DeleteAll<VideoItem>();
            }
        }

        public bool DoesMediaExist(String path)
        {
            return GetFromPath(path) != null;
        }

        public VideoItem GetFromPath(String path)
        {
            using (Connection.Lock())
            {
                return Connection.Table<VideoItem>().Where(x => x.Path == path).FirstOrDefault();
            }
        }

        public List<VideoItem> Load(Expression<Func<VideoItem, bool>> predicate)
        {
            using (Connection.Lock())
            {
                if (predicate == null)
                {
                    return Connection.Table<VideoItem>().ToList();
                }
                else
                {
                    return Connection.Table<VideoItem>().Where(predicate).ToList();
                }
            }
        }

        public VideoItem LoadVideo(int videoId)
        {
            using (Connection.Lock())
            {
                return Connection.Table<VideoItem>().Where(x => x.Id.Equals(videoId)).First();
            }
        }

        public void Insert(VideoItem item)
        {
            using (Connection.Lock())
            {
                Connection.Insert(item);
            }
        }

        public void Update(VideoItem video)
        {
            using (Connection.Lock())
            {
                Connection.Update(video);
            }
        }

        public void Remove(VideoItem video)
        {
            using (Connection.Lock())
            {
                Connection.Delete(video);
            }
        }

        public List<VideoItem> GetLastViewed()
        {
            using (Connection.Lock())
            {
                return Connection.Table<VideoItem>().Where(x => x.TimeWatchedSeconds > 0).ToList();
            }
        }


        public List<VideoItem> Contains(string column, string value)
        {
            using (Connection.Lock())
            {
                return Connection.Query<VideoItem>($"SELECT * FROM {nameof(VideoItem)} WHERE {column} LIKE '%{value}%';", new string[] { });
            }
        }

        public bool IsEmpty()
        {
            using (Connection.Lock())
            {
                return Connection.Table<VideoItem>().Count() == 0;
            }
        }
    }
}