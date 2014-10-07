using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using VLC_WINRT_APP.Model.Video;
using VLC_WINRT_APP.ViewModels.VideoVM;

namespace VLC_WINRT_APP.DataRepository
{
    public class LastVideosRepository : IDataRepository
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
                db.CreateTable<VideoVM>();
            }
        }

        public void Drop()
        {
            throw new System.NotImplementedException();
        }

        public
            async Task<ObservableCollection<VideoVM>> Load()
        {
            var connection = new SQLiteAsyncConnection(_dbPath);

            return new ObservableCollection<VideoVM>(
               await connection.QueryAsync<VideoVM>(
                     "select * from VideoVM"));
        }

        public async Task<VideoVM> LoadViaToken(string token)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<VideoVM>().Where(x => x.Token.Equals(token));
            var result = await query.ToListAsync();
            return result.FirstOrDefault();
        }

        public Task Update(VideoVM video)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.UpdateAsync(video);
        }

        public Task Add(VideoVM video)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.InsertAsync(video);
        }
    }
}
