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

        public async Task<bool> DoesTrackExist(string path)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            try
            {
                bool b = false;
                var connection = new SQLiteAsyncConnection(DbPath);
                var query = connection.Table<TrackItem>().Where(x => x.Path == path);
                b = await query.CountAsync() != 0;
               return b;
            }
            finally
            {
                MusicDatabase.DatabaseOperation.Release();
            }
        }

        public async Task<TrackItem> LoadTrackByPath(string path)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            try
            {
                var connection = new SQLiteAsyncConnection(DbPath);
                var query = connection.Table<TrackItem>().Where(x => x.Path == path);
                if (await query.CountAsync() > 0)
                {
                    var track = await query.FirstOrDefaultAsync();
                    return track;
                }
                return null;
            }
            finally
            {
                MusicDatabase.DatabaseOperation.Release();
            }
        }

        public async Task<TrackItem> LoadTrack(int trackId)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            try
            {
                var connection = new SQLiteAsyncConnection(DbPath);
                var query = connection.Table<TrackItem>().Where(x => x.Id.Equals(trackId));
                var result = await query.ToListAsync();
                return result.FirstOrDefault();
            }
            finally
            {
                MusicDatabase.DatabaseOperation.Release();
            }
        }

        public async Task<TrackItem> LoadTrack(int artistId, int albumId, string trackName)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            try
            {
                var connection = new SQLiteAsyncConnection(DbPath);
                var query = connection.Table<TrackItem>().Where(x => x.Name.Equals(trackName)).Where(x => x.ArtistId == artistId).Where(x => x.AlbumId == albumId);
                var result = await query.ToListAsync();
                return result.FirstOrDefault();
            }
            finally
            {
                MusicDatabase.DatabaseOperation.Release();
            }
        }

        public async Task<List<TrackItem>> LoadTracksByAlbumId(int albumId)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            try
            {
                using (var connection = new SQLiteConnection(DbPath))
                {
                    var query = connection.Table<TrackItem>().Where(x => x.AlbumId == albumId).OrderBy(x => x.DiscNumber).ThenBy(x => x.Index);
                    var tracks = query.ToList();
                    return tracks;
                }
            }
            finally
            {
                MusicDatabase.DatabaseOperation.Release();
            }
        }

        public async Task<TrackItem> GetFirstTrackOfAlbumId(int albumId)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            try
            {
                var connection = new SQLiteAsyncConnection(DbPath);
                var query = connection.Table<TrackItem>().Where(x => x.AlbumId == albumId);
                var result = await query.FirstOrDefaultAsync();
                return result;
            }
            finally
            {
                MusicDatabase.DatabaseOperation.Release();
            }
        }

        public async Task<string> GetFirstTrackPathByAlbumId(int albumId)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            try
            {
                var trackItem = await GetFirstTrackOfAlbumId(albumId);
                return trackItem?.Path;
            }
            finally
            {
                MusicDatabase.DatabaseOperation.Release();
            }
        }

        public async Task<List<TrackItem>> LoadTracksByArtistId(int artistId)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            try
            {
                var connection = new SQLiteAsyncConnection(DbPath);
                var query = connection.Table<TrackItem>().Where(x => x.ArtistId == artistId);
                var tracks = await query.ToListAsync();
                return tracks;
            }
            finally
            {
                MusicDatabase.DatabaseOperation.Release();
            }

        }

        public async Task<bool> IsEmpty()
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            try
            {
                var connection = new SQLiteAsyncConnection(DbPath);
                int c = await connection.Table<TrackItem>().CountAsync();
                return c == 0;
            }
            finally
            {
                MusicDatabase.DatabaseOperation.Release();
            }
        }

        public async Task<List<TrackItem>> LoadTracks()
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

        public async Task Update(TrackItem track)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            try
            {
                var connection = new SQLiteAsyncConnection(DbPath);
                await connection.UpdateAsync(track);
            }
            finally
            {
                MusicDatabase.DatabaseOperation.Release();
            }
        }

        public async Task Add(TrackItem track)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            try
            {
                var connection = new SQLiteAsyncConnection(DbPath);
                var query = connection.Table<TrackItem>().Where(x => x.Path == track.Path);
                var result = await query.ToListAsync();
                if (result.Count == 0)
                    await connection.InsertAsync(track);
            }
            finally
            {
                MusicDatabase.DatabaseOperation.Release();
            }
        }

        public Task Remove(TrackItem track)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.DeleteAsync(track);
        }
    }
}
