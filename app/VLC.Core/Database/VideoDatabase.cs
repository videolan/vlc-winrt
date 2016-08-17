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

        public VideoRepository()
        {
            Initialize();
        }

        public void Initialize()
        {
            using (var db = new SQLiteConnection(DbPath))
            {
                db.CreateTable<VideoItem>();
            }
        }

        public void Drop()
        {
            using (var db = new SQLiteConnection(DbPath))
            {
                db.DropTable<VideoItem>();
            }
        }

        public void DeleteAll()
        {
            using (var db = new SQLiteConnection(DbPath))
            {
                db.DeleteAll<VideoItem>();
            }
        }
        public async Task<bool> DoesMediaExist(String path)
        {
            var track = await GetFromPath(path);
            return track != null;
        }

        public async Task<VideoItem> GetFromPath(String path)
        {
            var conn = new SQLiteAsyncConnection(DbPath);
            var req = conn.Table<VideoItem>().Where(x => x.Path == path);
            var res = await req.ToListAsync().ConfigureAwait(false);
            return res.FirstOrDefault();
        }

        public async Task<List<VideoItem>> Load(Expression<Func<VideoItem, bool>> predicate)
        {
            var conn = new SQLiteAsyncConnection(DbPath);
            if (predicate == null)
            {
                return await conn.Table<VideoItem>().ToListAsync();
            }
            else
            {
                return await conn.Table<VideoItem>().Where(predicate).ToListAsync();
            }
        }

        public async Task<VideoItem> LoadVideo(int videoId)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            var query = connection.Table<VideoItem>().Where(x => x.Id.Equals(videoId));
            var result = await query.ToListAsync();
            return result.FirstOrDefault();
        }

        public Task Insert(VideoItem item)
        {
            var conn = new SQLiteAsyncConnection(DbPath);
            return conn.InsertAsync(item);
        }

        public Task Update(VideoItem video)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.UpdateAsync(video);
        }

        public Task<List<VideoItem>> GetLastViewed()
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            var req = connection.Table<VideoItem>().Where(x => x.TimeWatchedSeconds > 0);
            return req.ToListAsync();
        }


        public Task<List<VideoItem>> Contains(string column, string value)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.QueryAsync<VideoItem>($"SELECT * FROM {nameof(VideoItem)} WHERE {column} LIKE '%{value}%';", new string[] { });
        }

        public async Task<bool> IsEmpty()
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return await connection.Table<VideoItem>().CountAsync() == 0;
        }
    }
}