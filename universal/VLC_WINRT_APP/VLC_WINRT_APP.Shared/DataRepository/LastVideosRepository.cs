using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT_APP.ViewModels.VideoVM;

namespace VLC_WINRT_APP.DataRepository
{
    public class LastVideosRepository
    {
        private static readonly string _dbPath =
    Path.Combine(
    Windows.Storage.ApplicationData.Current.LocalFolder.Path,
    "mediavlcVideos.sqlite");
        public LastVideosRepository()
        {
            Initialize();
        }

        public void Initialize()
        {
            using (var db = new SQLiteConnection(_dbPath))
            {
                db.CreateTable<MediaViewModel>();
            }
        }

        public
            async Task<ObservableCollection<MediaViewModel>> Load()
        {
            var connection = new SQLiteAsyncConnection(_dbPath);

            return new ObservableCollection<MediaViewModel>(
               await connection.QueryAsync<MediaViewModel>(
                     "select * from MediaViewModel"));
        }

        public async Task<MediaViewModel> LoadViaToken(string token)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<MediaViewModel>().Where(x => x.Token.Equals(token));
            var result = await query.ToListAsync();
            return result.FirstOrDefault();
        }

        public Task Update(MediaViewModel video)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.UpdateAsync(video);
        }

        public Task Add(MediaViewModel video)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.InsertAsync(video);
        }
    }
}
