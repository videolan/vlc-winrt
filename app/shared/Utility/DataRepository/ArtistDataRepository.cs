using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using VLC_WINRT.ViewModels.MainPage;

namespace VLC_WINRT.Utility.DataRepository
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
            using (var db = new SQLite.SQLiteConnection(_dbPath))
            {
                db.CreateTable<MusicLibraryViewModel.ArtistItem>();
            }
        }

        public
            async Task<ObservableCollection<MusicLibraryViewModel.ArtistItem>> Load()
        {
            var connection = new SQLiteAsyncConnection(_dbPath);

            return new ObservableCollection<MusicLibraryViewModel.ArtistItem>(
               await connection.QueryAsync<MusicLibraryViewModel.ArtistItem>(
                     "select * from ArtistItem"));
        }
        public
    async Task<MusicLibraryViewModel.ArtistItem> LoadViaArtistName(string artistName)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<MusicLibraryViewModel.ArtistItem>().Where(x => x.Name.Equals(artistName));
            var result = await query.ToListAsync();
            return result.FirstOrDefault();
        }

        public Task Update(MusicLibraryViewModel.ArtistItem artist)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.UpdateAsync(artist);
        }

        public Task Add(MusicLibraryViewModel.ArtistItem artist)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.InsertAsync(artist);
        }
    }
}
