using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SQLite;
using VLC_WinRT.Model.Music;
using System.Collections.Generic;
using VLC_WinRT.Utils;

namespace VLC_WinRT.DataRepository
{
    public class AlbumDataRepository : IDataRepository
    {
        private static readonly string DbPath = Strings.MusicDatabase;

        public AlbumDataRepository()
        {
            Initialize();
        }
        public void Initialize()
        {
            using (var db = new SQLite.SQLiteConnection(DbPath))
            {
                db.CreateTable<AlbumItem>();
            }
        }

        public void Drop()
        {
            using (var db = new SQLite.SQLiteConnection(DbPath))
            {
                db.DropTable<AlbumItem>();
            }
        }

        public async Task<List<AlbumItem>> LoadAlbumsFromId(int artistId)
        {
            var connection = new SQLiteAsyncConnection(DbPath);

            return await connection.Table<AlbumItem>().Where(x => x.ArtistId == artistId).ToListAsync();

        }

        public async Task<AlbumItem> LoadAlbumViaName(int artistId, string albumName)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            var query = connection.Table<AlbumItem>().Where(x => x.Name.Equals(albumName)).Where(x => x.ArtistId == artistId);
            var result = await query.ToListAsync();
            return result.FirstOrDefault();
        }

        public async Task<AlbumItem> LoadAlbum(int albumId)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            var query = connection.Table<AlbumItem>().Where(x => x.Id.Equals(albumId));
            var result = await query.ToListAsync();
            return result.FirstOrDefault();
        }

        public async Task<List<AlbumItem>> LoadAlbums(Expression<Func<AlbumItem, bool>> compare)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            var query = connection.Table<AlbumItem>().Where(compare);
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
