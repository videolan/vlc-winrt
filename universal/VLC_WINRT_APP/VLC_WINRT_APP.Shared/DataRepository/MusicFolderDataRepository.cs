using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.SmartCards;
using Windows.UI.Core;
using SQLite;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.DataRepository
{
    public class MusicFolderDataRepository : IDataRepository
    {
        private static readonly string _dbPath =
    Path.Combine(
    Windows.Storage.ApplicationData.Current.LocalFolder.Path,
    "musicFolderVlc.sqlite");

        public MusicFolderDataRepository()
        {
            Initialize();
        }

        public void Initialize()
        {
            using (var db = new SQLiteConnection(_dbPath))
            {
                db.CreateTable<VLCFolder>();
            }
        }

        public void Drop()
        {
            using (var db = new SQLiteConnection(_dbPath))
            {
                db.DropTable<VLCFolder>();
            }
        }

        public Task Add(VLCFolder folder)
        {
            var connexion = new SQLiteAsyncConnection(_dbPath);
            return connexion.InsertAsync(folder);
        }

        public Task Remove(VLCFolder folder)
        {
            var connexion = new SQLiteAsyncConnection(_dbPath);
            return connexion.DeleteAsync(folder);
        }

        public async Task<List<VLCFolder>> Load()
        {
            var connexion = new SQLiteAsyncConnection(_dbPath);
            var query = connexion.Table<VLCFolder>();
            var result = await query.ToListAsync();
            return result;
        }

        public async Task<bool> Exist(string path)
        {
            var connexion = new SQLiteAsyncConnection(_dbPath);
            var query = connexion.Table<VLCFolder>().Where(x=>x.Path == path);
            var result = await query.ToListAsync();
            return result.Count > 0;
        }

        public async Task<VLCFolder> LoadFolder(string path)
        {
            var connexion = new SQLiteAsyncConnection(_dbPath);
            var query = connexion.Table<VLCFolder>().Where(x=>x.Path == path);
            var result = await query.ToListAsync();
            return result.FirstOrDefault();
        }

        public Task Update(VLCFolder folder)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.UpdateAsync(folder);
        }
    }
}
