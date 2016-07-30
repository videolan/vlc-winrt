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

        public void DeleteAll()
        {
            using (var db = new SQLiteConnection(DbPath))
            {
                db.DeleteAll<ArtistItem>();
            }
        }

        public async Task<int> Count()
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            var connection = new SQLiteAsyncConnection(DbPath);
            var count = await connection.Table<ArtistItem>().CountAsync();
            MusicDatabase.DatabaseOperation.Release();
            return count;
        }

        public async Task<ArtistItem> At(int index)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            var connection = new SQLiteAsyncConnection(DbPath);
            var artist = await connection.Table<ArtistItem>().ElementAtAsync(index);
            MusicDatabase.DatabaseOperation.Release();
            return artist;
        }

        public async Task<List<ArtistItem>> Load(Expression<Func<ArtistItem, bool>> compare = null)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
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
            var artists = await query.ToListAsync();
            MusicDatabase.DatabaseOperation.Release();
            return artists;
        }

        public async Task<ArtistItem> LoadViaArtistName(string artistName)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            using (var db = new SQLiteConnection(DbPath))
            {
                var query = db.Table<ArtistItem>().Where(x => x.Name.Equals(artistName));
                var artist= query.FirstOrDefault();
                MusicDatabase.DatabaseOperation.Release();
                return artist;
            }
        }

        public async Task Update(ArtistItem artist)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            await connection.UpdateAsync(artist);
        }

        public async Task Add(ArtistItem artist)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            var connection = new SQLiteAsyncConnection(DbPath);
            await connection.InsertAsync(artist);
            MusicDatabase.DatabaseOperation.Release();
        }

        public async Task<ArtistItem> LoadArtist(int artistId)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            var connection = new SQLiteAsyncConnection(DbPath);
            var query = connection.Table<ArtistItem>().Where(x => x.Id.Equals(artistId));
            var result = await query.ToListAsync();
            var artist= result.FirstOrDefault();
            MusicDatabase.DatabaseOperation.Release();
            return artist;
        }

        public async Task Remove(ArtistItem artist)
        {
            await MusicDatabase.DatabaseOperation.WaitAsync();
            var connection = new SQLiteAsyncConnection(DbPath);
            await connection.DeleteAsync(artist);
            MusicDatabase.DatabaseOperation.Release();
        }
    }
}
