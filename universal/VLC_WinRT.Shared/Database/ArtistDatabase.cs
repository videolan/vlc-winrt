using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using VLC_WinRT.Model.Music;
using System.Collections.Generic;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Database
{
    public class ArtistDatabase : IDatabase
    {
        private static readonly string DbPath = Strings.MusicDatabase;

        private SQLiteConnection connection;
        public ArtistDatabase()
        {
            Initialize();
        }

        public void Initialize()
        {
            var db = new SQLiteConnection(DbPath);
            connection = db;
            connection.CreateTable<ArtistItem>();
        }

        public void Drop()
        {
            using (var db = new SQLiteConnection(DbPath))
            {
                db.DropTable<ArtistItem>();
            }
        }

        public async Task<List<ArtistItem>> Load()
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return await connection.Table<ArtistItem>().ToListAsync();
        }

        public ArtistItem LoadViaArtistName(string artistName)
        {
            var query = connection.Table<ArtistItem>().Where(x => x.Name.Equals(artistName));
            return query.FirstOrDefault();
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
