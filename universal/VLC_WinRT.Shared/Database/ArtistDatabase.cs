using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using VLC_WinRT.Model.Music;
using System.Collections.Generic;
using VLC_WinRT.Utils;
using System.Linq.Expressions;
using System;

namespace VLC_WinRT.Database
{
    public class ArtistDatabase : IDatabase
    {
        private static readonly string DbPath = Strings.MusicDatabase;
        
        public ArtistDatabase()
        {
            Initialize();
        }

        public void Initialize()
        {
            using (var db = new SQLiteConnection(DbPath))
            {
                db.CreateTable<ArtistItem>();
            }
        }

        public void Drop()
        {
            using (var db = new SQLiteConnection(DbPath))
            {
                db.DropTable<ArtistItem>();
            }
        }

        public async Task<List<ArtistItem>> Load(Expression<Func<ArtistItem, bool>> compare = null)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            AsyncTableQuery<ArtistItem> query;
            if (compare == null)
            {
                query = connection.Table<ArtistItem>();
            }
            else
            {
                query = connection.Table<ArtistItem>().Where(compare);
            }
            return await query.ToListAsync();
        }

        public ArtistItem LoadViaArtistName(string artistName)
        {
            using (var db = new SQLiteConnection(DbPath))
            {
                var query = db.Table<ArtistItem>().Where(x => x.Name.Equals(artistName));
                return query.FirstOrDefault();
            }
        }

        public Task Update(ArtistItem artist)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.UpdateAsync(artist);
        }

        public Task Add(ArtistItem artist)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.InsertAsync(artist);
        }

        public async Task<ArtistItem> LoadArtist(int artistId)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            var query = connection.Table<ArtistItem>().Where(x => x.Id.Equals(artistId));
            var result = await query.ToListAsync();
            return result.FirstOrDefault();
        }

        public Task Remove(ArtistItem artist)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.DeleteAsync(artist);
        }
    }
}
