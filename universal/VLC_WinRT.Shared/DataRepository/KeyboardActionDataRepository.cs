using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using SQLite;
using VLC_WinRT.Model;

namespace VLC_WinRT.DataRepository
{
    public class KeyboardActionDataRepository : IDataRepository
    {
        private static readonly string _dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "settings.sqlite");

        public KeyboardActionDataRepository()
        {
            Initialize();
        }

        public void Initialize()
        {
            using (var db = new SQLiteConnection(_dbPath))
            {
                db.CreateTable<KeyboardAction>();
            }
        }

        public Task AddKeyboardActions(List<KeyboardAction> keyboardActions)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.InsertAllAsync(keyboardActions);
        }

        public Task UpdateKeyboardAction(KeyboardAction keyboardAction)
        {
            //one shortcut per action, so only updates
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.UpdateAsync(keyboardAction);
        }

        public Task<List<KeyboardAction>> GetAllKeyboardActions()
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.Table<KeyboardAction>().ToListAsync();
        }

        public Task<KeyboardAction> GetKeyboardAction(VirtualKey mainpressedKey, VirtualKey secondKey)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.Table<KeyboardAction>().Where(x => x.MainKey == mainpressedKey && x.SecondKey == secondKey).FirstOrDefaultAsync();
        }

        public void Drop()
        {
            throw new NotImplementedException();
        }
    }
}
