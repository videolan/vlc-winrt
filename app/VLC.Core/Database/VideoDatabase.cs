using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SQLite;
using VLC.Model.Stream;
using VLC.Model.Video;
using VLC.Utils;

namespace VLC.Database
{
    public class VideoDatabase : IDatabase
    {
        private static readonly string DbPath = Strings.VideoDatabase;

        private SQLiteConnectionWithLock connection = new SQLiteConnectionWithLock(new SQLiteConnectionString(DbPath, false),
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex | SQLiteOpenFlags.SharedCache);

        public VideoDatabase()
        {
            Initialize();
        }

        public void Initialize()
        {
            using (connection.Lock())
            {
                connection.CreateTable<VideoItem>();
                connection.CreateTable<StreamMedia>();
            }
        }

        public void Drop()
        {
            using (connection.Lock())
            {
                connection.DropTable<VideoItem>();
                connection.DropTable<StreamMedia>();
            }
        }

        public void DeleteAll()
        {
            using (connection.Lock())
            {
                connection.DeleteAll<VideoItem>();
                connection.DeleteAll<StreamMedia>();
            }
        }

        #region VideoItem
        public bool DoesMediaExist(String path)
        {
            return GetFromPath(path) != null;
        }

        public VideoItem GetFromPath(String path)
        {
            using (connection.Lock())
            {
                return connection.Table<VideoItem>().FirstOrDefault(x => x.Path == path);
            }
        }

        public List<VideoItem> Load(Expression<Func<VideoItem, bool>> predicate)
        {
            using (connection.Lock())
            {
                if (predicate == null)
                {
                    return connection.Table<VideoItem>().ToList();
                }
                else
                {
                    return connection.Table<VideoItem>().Where(predicate).ToList();
                }
            }
        }

        public VideoItem LoadVideo(int videoId)
        {
            using (connection.Lock())
            {
                return connection.Table<VideoItem>().Where(x => x.Id.Equals(videoId)).First();
            }
        }

        public void Insert(VideoItem item)
        {
            using (connection.Lock())
            {
                connection.Insert(item);
            }
        }

        public void Update(VideoItem video)
        {
            using (connection.Lock())
            {
                connection.Update(video);
            }
        }

        public void Remove(VideoItem video)
        {
            using (connection.Lock())
            {
                connection.Delete(video);
            }
        }

        public List<VideoItem> GetLastViewed()
        {
            using (connection.Lock())
            {
                return connection.Table<VideoItem>().Where(x => x.TimeWatchedSeconds > 0).ToList();
            }
        }


        public List<VideoItem> Contains(string column, string value)
        {
            using (connection.Lock())
            {
                return connection.Query<VideoItem>($"SELECT * FROM {nameof(VideoItem)} WHERE {column} LIKE '%{value}%';", new string[] { });
            }
        }

        public bool IsEmpty()
        {
            using (connection.Lock())
            {
                return connection.Table<VideoItem>().Count() == 0;
            }
        }
        #endregion

        #region StreamMedia
        public List<StreamMedia> LoadStreams()
        {
            using (connection.Lock())
            {
                return connection.Table<StreamMedia>().ToList();
            }
        }

        public void Insert(StreamMedia stream)
        {
            using (connection.Lock())
            {
                connection.Insert(stream);
            }
        }

        public void Update(StreamMedia stream)
        {
            using (connection.Lock())
            {
                connection.Update(stream);
            }
        }

        public StreamMedia GetStream(string media)
        {
            using (connection.Lock())
            {
                return connection.Find<StreamMedia>(x => x.Path == media);
            }
        }

        public void Delete(StreamMedia stream)
        {
            using (connection.Lock())
            {
                connection.Delete(stream);
            }
        }

        public bool Contains(StreamMedia stream)
        {
            using (connection.Lock())
            {
                return GetStream(stream.Path) != null;
            }
        }
        #endregion
    }
}