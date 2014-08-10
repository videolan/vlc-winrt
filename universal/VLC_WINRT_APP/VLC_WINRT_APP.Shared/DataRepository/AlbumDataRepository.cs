using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.DataRepository
{
    public class AlbumDataRepository
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
                db.CreateTable<MusicLibraryVM.AlbumItem>();

            }
        }

        public void Drop()
        {
            using (var db = new SQLite.SQLiteConnection(_dbPath))
            {
                db.DropTable<MusicLibraryVM.AlbumItem>();
            }
        }

        public async Task<ObservableCollection<MusicLibraryVM.AlbumItem>> LoadAlbumsFromId(int artistId)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);

            return new ObservableCollection<MusicLibraryVM.AlbumItem>(
               await connection.QueryAsync<MusicLibraryVM.AlbumItem>(
                     string.Format("select * from AlbumItem where ArtistId = {0}", artistId)));

        }

        public async Task<MusicLibraryVM.AlbumItem> LoadAlbumViaName(int artistId, string albumName)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<MusicLibraryVM.AlbumItem>().Where(x => x.Name.Equals(albumName)).Where(x => x.ArtistId == artistId);
            var result = await query.ToListAsync();
            return result.FirstOrDefault();
        }

        public async Task<MusicLibraryVM.AlbumItem> LoadAlbum(int albumId)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<MusicLibraryVM.AlbumItem>().Where(x => x.Id.Equals(albumId));
            var result = await query.ToListAsync();
            return result.FirstOrDefault();
        }

        public Task Update(MusicLibraryVM.AlbumItem album)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.UpdateAsync(album);
        }

        public Task Add(MusicLibraryVM.AlbumItem album)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.InsertAsync(album);
        }
    }
}
