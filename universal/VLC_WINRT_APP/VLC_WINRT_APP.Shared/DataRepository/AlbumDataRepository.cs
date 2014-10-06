using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.DataRepository
{
    public class AlbumDataRepository : IDataRepository
    {
        private static readonly string _dbPath =
    Path.Combine(
    Windows.Storage.ApplicationData.Current.LocalFolder.Path,
    "mediavlc.sqlite");

        public AlbumDataRepository()
        {
            Initialize();
        }
        public void Initialize()
        {
            using (var db = new SQLite.SQLiteConnection(_dbPath))
            {
                db.CreateTable<AlbumItem>();

            }
        }

        public void Drop()
        {
            using (var db = new SQLite.SQLiteConnection(_dbPath))
            {
                db.DropTable<AlbumItem>();
            }
        }

        public async Task<ObservableCollection<AlbumItem>> LoadAlbumsFromId(int artistId)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);

            return new ObservableCollection<AlbumItem>(
               await connection.QueryAsync<AlbumItem>(
                     string.Format("select * from AlbumItem where ArtistId = {0}", artistId)));

        }

        public async Task<AlbumItem> LoadAlbumViaName(int artistId, string albumName)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<AlbumItem>().Where(x => x.Name.Equals(albumName)).Where(x => x.ArtistId == artistId);
            var result = await query.ToListAsync();
            return result.FirstOrDefault();
        }

        public async Task<AlbumItem> LoadAlbum(int albumId)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<AlbumItem>().Where(x => x.Id.Equals(albumId));
            var result = await query.ToListAsync();
            return result.FirstOrDefault();
        }

        public Task Update(AlbumItem album)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.UpdateAsync(album);
        }

        public Task Add(AlbumItem album)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.InsertAsync(album);
        }

        public void Remove(AlbumItem album)
        {
            if (album == null) return;
            var connection = new SQLiteAsyncConnection(_dbPath);
            connection.DeleteAsync(album);
        }
    }
}
