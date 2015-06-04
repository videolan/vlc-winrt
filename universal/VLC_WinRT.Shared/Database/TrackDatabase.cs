using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using VLC_WinRT.Model.Music;
using System.Collections.Generic;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Database
{
    public class TrackDatabase : IDatabase
    {
        private static readonly string DbPath = Strings.MusicDatabase;

        public TrackDatabase()
        {
            Initialize();
        }

        public void Initialize()
        {
            var db = new SQLiteConnection(DbPath);
            db.CreateTable<TrackItem>();
        }

        public void Drop()
        {
            using (var db = new SQLiteConnection(DbPath))
            {
                db.DropTable<TrackItem>();
            }
        }

        public async Task<bool> DoesTrackExist(string path)
        {
            bool b = false;
            var connection = new SQLiteAsyncConnection(DbPath);
            var query = connection.Table<TrackItem>().Where(x => x.Path == path);
            b = await query.CountAsync() != 0;
            return b;
        }

        public async Task<TrackItem> LoadTrackByPath(string path)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            var query = connection.Table<TrackItem>().Where(x => x.Path == path);
            if (await query.CountAsync() > 0)
            {
                return await query.FirstOrDefaultAsync();
            }
            return null;
        }

        public async Task<TrackItem> LoadTrack(int trackId)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            var query = connection.Table<TrackItem>().Where(x => x.Id.Equals(trackId));
            var result = await query.ToListAsync();
            return result.FirstOrDefault();
        }

        public async Task<TrackItem> LoadTrack(int artistId, int albumId, string trackName)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            var query = connection.Table<TrackItem>().Where(x => x.Name.Equals(trackName)).Where(x => x.ArtistId == artistId).Where(x => x.AlbumId == albumId);
            var result = await query.ToListAsync();
            return result.FirstOrDefault();
        }

        public List<TrackItem> LoadTracksByAlbumId(int albumId)
        {
            using (var connection = new SQLiteConnection(DbPath))
            {
                var query = connection.Table<TrackItem>().Where(x => x.AlbumId == albumId).OrderBy(x => x.DiscNumber).ThenBy(x => x.Index);
                return query.ToList();
            }
        }

        public async Task<string> GetFirstTrackPathByAlbumId(int albumId)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            var query = connection.Table<TrackItem>().Where(x => x.AlbumId == albumId);
            var result = await query.FirstOrDefaultAsync();
            return result != null ? result.Path : null;
        }

        public async Task<List<TrackItem>> LoadTracksByArtistId(int artistId)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            var query = connection.Table<TrackItem>().Where(x => x.ArtistId == artistId);
            return await query.ToListAsync();
        }

        public async Task<List<TrackItem>> LoadTracks()
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            var query = connection.Table<TrackItem>().OrderBy(x => x.Name);
            return await query.ToListAsync();
        }

        public Task Update(TrackItem track)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.UpdateAsync(track);
        }

        public async Task Add(TrackItem track)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            var query = connection.Table<TrackItem>().Where(x => x.Path == track.Path);
            var result = await query.ToListAsync();
            if (result.Count == 0)
                await connection.InsertAsync(track);
        }

        public Task Remove(TrackItem track)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.DeleteAsync(track);
        }
    }
}
