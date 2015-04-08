using System.IO;
using System.Threading.Tasks;
using SQLite;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels.MusicVM;
using System.Collections.Generic;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.DataRepository
{
    public class TrackCollectionRepository : IDataRepository
    {
        private static readonly string DbPath = Strings.MusicDatabase;

        public TrackCollectionRepository()
        {
            Initialize();
        }

        public void Initialize()
        {
            using (var db = new SQLiteConnection(DbPath))
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
            var loadTracks = await Locator.MusicLibraryVM.TracklistItemRepository.LoadTracks(trackCollection);
            foreach (TracklistItem tracklistItem in loadTracks)
            {
                await Locator.MusicLibraryVM.TracklistItemRepository.Remove(tracklistItem);
            }
            await connection.DeleteAsync(trackCollection);
        }

        public void Drop()
        {
            using (var db = new SQLiteConnection(DbPath))
            {
                db.DropTable<TrackCollection>();
            }
        }
    }
}
