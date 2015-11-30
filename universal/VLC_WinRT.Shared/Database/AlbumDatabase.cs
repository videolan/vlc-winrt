using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SQLite;
using VLC_WinRT.Model.Music;
using System.Collections.Generic;
using VLC_WinRT.Utils;

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

        public Task<List<AlbumItem>> LoadAlbumsFromId(int artistId)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.Table<AlbumItem>().Where(x => x.ArtistId == artistId).OrderBy(x => x.Name).ToListAsync();
        }

        public async Task<AlbumItem> LoadAlbumViaName(int artistId, string albumName)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            var query = connection.Table<AlbumItem>().Where(x => x.Name.Equals(albumName)).Where(x => x.ArtistId == artistId);
            var result = await query.ToListAsync();
            return result.FirstOrDefault();
        }

        public AlbumItem LoadAlbum(int albumId)
        {
            using (var connection = new SQLiteConnection(DbPath))
            {
                var query = connection.Table<AlbumItem>().Where(x => x.Id.Equals(albumId));
                var result = query.ToList();
                return result.FirstOrDefault();
            }
        }

        public async Task<List<AlbumItem>> LoadAlbums(Expression<Func<AlbumItem, bool>> compare = null)
        {
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
            return await query.ToListAsync();
        }

        public async Task<List<AlbumItem>> Load(Expression<Func<AlbumItem, bool>> compare = null)
        {
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
            return await query.ToListAsync();
        }

        public Task Update(AlbumItem album)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.UpdateAsync(album);
        }

        public Task Add(AlbumItem album)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.InsertAsync(album);
        }

        public void Remove(AlbumItem album)
        {
            if (album == null) return;
            var connection = new SQLiteAsyncConnection(DbPath);
            connection.DeleteAsync(album);
        }
    }
}
