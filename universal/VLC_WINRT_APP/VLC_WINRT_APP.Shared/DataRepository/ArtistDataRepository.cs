using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using VLC_WinRT.Model.Music;
using System.Collections.Generic;

namespace VLC_WinRT.DataRepository
{
    public class ArtistDataRepository : IDataRepository
    {
        private static readonly string _dbPath =
    Path.Combine(
    Windows.Storage.ApplicationData.Current.LocalFolder.Path,
    "mediavlc.sqlite");

        private SQLiteConnection connection;
        public ArtistDataRepository()
        {
            Initialize();
        }

        public void Initialize()
        {
            var db = new SQLiteConnection(_dbPath);
            connection = db;
            connection.CreateTable<ArtistItem>();
        }

        public void Drop()
        {
            using (var db = new SQLite.SQLiteConnection(_dbPath))
            {
                db.DropTable<ArtistItem>();
            }
        }

        public async Task<List<ArtistItem>> Load()
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return await connection.Table<ArtistItem>().ToListAsync();
        }

        public ArtistItem LoadViaArtistName(string artistName)
        {
            var query = connection.Table<ArtistItem>().Where(x => x.Name.Equals(artistName));
            return query.FirstOrDefault();
        }

        public Task Update(ArtistItem artist)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.UpdateAsync(artist);
        }

        public Task Add(ArtistItem artist)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.InsertAsync(artist);
        }

        public async Task<ArtistItem> LoadArtist(int artistId)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<ArtistItem>().Where(x => x.Id.Equals(artistId));
            var result = await query.ToListAsync();
            return result.FirstOrDefault();
        }

        public Task Remove(ArtistItem artist)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.DeleteAsync(artist);
        }
    }
}
