using System.IO;
using System.Threading.Tasks;
using SQLite;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels.MusicVM;
using System.Collections.Generic;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Model;

namespace VLC_WinRT.Database
{
    public class TrackCollectionRepository : IDatabase
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
                db.CreateTable<PlaylistItem>();
            }
        }

        public void DeleteAll()
        {
            using (var db = new SQLiteConnection(DbPath))
            {
                db.DeleteAll<PlaylistItem>();
            }
        }

        public async Task<PlaylistItem> LoadFromName(string name)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return await connection.Table<PlaylistItem>().Where(x => x.Name == name).FirstOrDefaultAsync();
        }

        public async Task<List<PlaylistItem>> LoadTrackCollections()
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return await connection.Table<PlaylistItem>().ToListAsync();
        }

        public Task Add(PlaylistItem trackCollection)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.InsertAsync(trackCollection);
        }

        public async Task Remove(PlaylistItem trackCollection)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            var loadTracks = await Locator.MediaLibrary.LoadTracks(trackCollection);
            foreach (TracklistItem tracklistItem in loadTracks)
            {
                await Locator.MediaLibrary.Remove(tracklistItem);
            }
            await connection.DeleteAsync(trackCollection);
        }

        public void Drop()
        {
            using (var db = new SQLiteConnection(DbPath))
            {
                db.DropTable<PlaylistItem>();
            }
        }
    }
}
