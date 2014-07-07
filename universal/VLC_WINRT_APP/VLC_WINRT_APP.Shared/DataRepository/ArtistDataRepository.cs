using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.DataRepository
{
    public class ArtistDataRepository
    {
        private static readonly string _dbPath =
    Path.Combine(
    Windows.Storage.ApplicationData.Current.LocalFolder.Path,
    "mediavlc.sqlite");
        public ArtistDataRepository()
        {
            Initialize();
        }

        public void Initialize()
        {
            using (var db = new SQLiteConnection(_dbPath))
            {
                db.CreateTable<MusicLibraryVM.ArtistItem>();
            }
        }

        public
            async Task<ObservableCollection<MusicLibraryVM.ArtistItem>> Load()
        {
            var connection = new SQLiteAsyncConnection(_dbPath);

            return new ObservableCollection<MusicLibraryVM.ArtistItem>(
               await connection.QueryAsync<MusicLibraryVM.ArtistItem>(
                     "select * from ArtistItem"));
        }
        public async Task<MusicLibraryVM.ArtistItem> LoadViaArtistName(string artistName)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<MusicLibraryVM.ArtistItem>().Where(x => x.Name.Equals(artistName));
            var result = await query.ToListAsync();
            return result.FirstOrDefault();
        }

        public Task Update(MusicLibraryVM.ArtistItem artist)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.UpdateAsync(artist);
        }

        public Task Add(MusicLibraryVM.ArtistItem artist)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.InsertAsync(artist);
        }

        public async Task<MusicLibraryVM.ArtistItem> LoadArtist(int artistId)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<MusicLibraryVM.ArtistItem>().Where(x => x.Id.Equals(artistId));
            var result = await query.ToListAsync();
            return result.FirstOrDefault();
        }
    }
}
