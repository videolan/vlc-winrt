using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SQLite;
using VLC_WinRT.Model.Music;
using System.Collections.Generic;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Database
{
    public class AlbumDatabase : IDatabase
    {
        private static readonly string DbPath = Strings.MusicDatabase;

        public AlbumDatabase()
        {
            Initialize();
        }

        public void Initialize()
        {
            using (var db = new SQLiteConnection(DbPath))
            {
                db.CreateTable<AlbumItem>();
            }
        }

        public void Drop()
        {
            using (var db = new SQLiteConnection(DbPath))
            {
                db.DropTable<AlbumItem>();
            }
        }

        public void DeleteAll()
        {
            using (var db = new SQLiteConnection(DbPath))
            {
                db.DeleteAll<AlbumItem>();
            }
        }

        public async Task<List<AlbumItem>> LoadAlbumsFromId(int artistId)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            using (var connection = new SQLiteConnection(DbPath))
            {
                var albums = connection.Table<AlbumItem>().Where(x => x.ArtistId == artistId).OrderBy(x => x.Name).ToList();
                MusicDatabase.DatabaseOperation.Release();
                return albums;
            }
        }

        public async Task<List<AlbumItem>> LoadAlbumsFromIdWithTracks(int artistId)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            using (var connection = new SQLiteConnection(DbPath))
            {
                var albums = connection.Table<AlbumItem>().Where(x => x.ArtistId == artistId).OrderBy(x => x.Name).ToList();
                foreach (var album in albums)
                {
                    var tracks = connection.Table<TrackItem>().Where(x => x.AlbumId == album.Id).OrderBy(x => x.DiscNumber).ThenBy(x => x.Index).ToList();
                    album.Tracks = tracks;
                }
                MusicDatabase.DatabaseOperation.Release();
                return albums;
            }
        }

        public async Task<int> LoadAlbumsCountFromId(int artistId)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            var connection = new SQLiteAsyncConnection(DbPath);
            var count= await connection.Table<AlbumItem>().Where(x => x.ArtistId == artistId).CountAsync();
            MusicDatabase.DatabaseOperation.Release();
            return count;
        }

        public async Task<AlbumItem> LoadAlbumViaName(int artistId, string albumName)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            var connection = new SQLiteAsyncConnection(DbPath);
            var query = connection.Table<AlbumItem>().Where(x => x.Name.Equals(albumName)).Where(x => x.ArtistId == artistId);
            var result = await query.ToListAsync();
            var album= result.FirstOrDefault();
            MusicDatabase.DatabaseOperation.Release();
            return album;
        }

        public async Task<AlbumItem> LoadAlbum(int albumId)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            using (var connection = new SQLiteConnection(DbPath))
            {
                var query = connection.Table<AlbumItem>().Where(x => x.Id.Equals(albumId));
                var result = query.ToList();
                var album =result.FirstOrDefault();
                MusicDatabase.DatabaseOperation.Release();
                return album;
            }
        }

        public async Task<List<AlbumItem>> LoadAlbums(Expression<Func<AlbumItem, bool>> compare = null)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            var connection = new SQLiteAsyncConnection(DbPath);
            AsyncTableQuery<AlbumItem> query;
            if (compare == null)
            {
                query = connection.Table<AlbumItem>();
            }
            else
            {
                query = connection.Table<AlbumItem>().Where(compare);
            }
            var albums= await query.ToListAsync();
            MusicDatabase.DatabaseOperation.Release();
            return albums;
        }

        public async Task<List<AlbumItem>> Load(Expression<Func<AlbumItem, bool>> compare = null)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            var connection = new SQLiteAsyncConnection(DbPath);
            AsyncTableQuery<AlbumItem> query;
            if (compare == null)
            {
                query = connection.Table<AlbumItem>();
            }
            else
            {
                query = connection.Table<AlbumItem>().Where(compare);
            }
            var albums= await query.ToListAsync();
            MusicDatabase.DatabaseOperation.Release();
            return albums;
        }

        public async Task<List<AlbumItem>> Contains(string column, string value)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            var connection = new SQLiteAsyncConnection(DbPath);
            var list = await connection.QueryAsync<AlbumItem>($"SELECT * FROM {nameof(AlbumItem)} WHERE {column} LIKE '%{value}%';", new string[] { });
            MusicDatabase.DatabaseOperation.Release();
            return list;
        }

        public async Task Update(AlbumItem album)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            var connection = new SQLiteAsyncConnection(DbPath);
            await connection.UpdateAsync(album);
            MusicDatabase.DatabaseOperation.Release();
        }

        public async Task Add(AlbumItem album)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            var connection = new SQLiteAsyncConnection(DbPath);
            await connection.InsertAsync(album);
            MusicDatabase.DatabaseOperation.Release();
        }

        public void Remove(AlbumItem album)
        {
            if (album == null) return;
            var connection = new SQLiteAsyncConnection(DbPath);
            connection.DeleteAsync(album);
        }
    }
}
