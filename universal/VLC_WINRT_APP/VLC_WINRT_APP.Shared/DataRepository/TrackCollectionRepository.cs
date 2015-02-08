using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using SQLite;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels.MusicVM;
using System.Collections.Generic;

namespace VLC_WINRT_APP.DataRepository
{
    public class TrackCollectionRepository : IDataRepository
    {
        private static readonly string DbPath =
   Path.Combine(
   Windows.Storage.ApplicationData.Current.LocalFolder.Path,
   "mediavlc.sqlite");

        public TrackCollectionRepository()
        {
            Initialize();
        }

        public void Initialize()
        {
            using (var db = new SQLite.SQLiteConnection(DbPath))
            {
                db.CreateTable<TrackCollection>();
            }
        }

        public async Task<TrackCollection> LoadFromName(string name)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return await connection.Table<TrackCollection>().Where(x => x.Name == name).FirstOrDefaultAsync();
        }

        public async Task<List<TrackCollection>> LoadTrackCollections()
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return await connection.Table<TrackCollection>().ToListAsync();
        }

        public Task Add(TrackCollection trackCollection)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.InsertAsync(trackCollection);
        }

        public async Task Remove(TrackCollection trackCollection)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            var loadTracks = await MusicLibraryVM.TracklistItemRepository.LoadTracks(trackCollection);
            foreach (TracklistItem tracklistItem in loadTracks)
            {
                await MusicLibraryVM.TracklistItemRepository.Remove(tracklistItem);
            }
            await connection.DeleteAsync(trackCollection);
        }

        public void Drop()
        {
            using (var db = new SQLite.SQLiteConnection(DbPath))
            {
                db.DropTable<TrackCollection>();
            }
        }
    }
}
