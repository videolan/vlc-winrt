using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT_APP.ViewModels.MainPage;

namespace VLC_WINRT.Utility.DataRepository
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
                db.CreateTable<MusicLibraryViewModel.AlbumItem>();

            }
        }

        public
            async Task<ObservableCollection<MusicLibraryViewModel.AlbumItem>> LoadAlbumsFromId(int artistId)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);

            return new ObservableCollection<MusicLibraryViewModel.AlbumItem>(
               await connection.QueryAsync<MusicLibraryViewModel.AlbumItem>(
                     string.Format("select * from AlbumItem where ArtistId = {0}", artistId)));

        }

        public
    async Task<MusicLibraryViewModel.AlbumItem> LoadAlbumViaName(int artistId, string albumName)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<MusicLibraryViewModel.AlbumItem>().Where(x => x.Name.Equals(albumName)).Where(x => x.ArtistId == artistId);
            var result = await query.ToListAsync();
            return result.FirstOrDefault();
        }

        public Task Update(MusicLibraryViewModel.AlbumItem album)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.UpdateAsync(album);
        }

        public Task Add(MusicLibraryViewModel.AlbumItem album)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.InsertAsync(album);
        }
    }
}
