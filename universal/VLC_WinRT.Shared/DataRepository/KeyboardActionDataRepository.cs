using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using SQLite;
using VLC_WinRT.Model;
using VLC_WinRT.Utils;

namespace VLC_WinRT.DataRepository
{
    public class KeyboardActionDataRepository : IDataRepository
    {
        private static readonly string DbPath = Strings.SettingsDatabase;

        public KeyboardActionDataRepository()
        {
            Initialize();
        }

        public void Initialize()
        {
            using (var db = new SQLiteConnection(DbPath))
            {
                db.CreateTable<KeyboardAction>();
            }
        }

        public Task AddKeyboardActions(List<KeyboardAction> keyboardActions)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.InsertAllAsync(keyboardActions);
        }

        public Task UpdateKeyboardAction(KeyboardAction keyboardAction)
        {
            //one shortcut per action, so only updates
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.UpdateAsync(keyboardAction);
        }

        public Task<List<KeyboardAction>> GetAllKeyboardActions()
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.Table<KeyboardAction>().ToListAsync();
        }

        public Task<KeyboardAction> GetKeyboardAction(VirtualKey mainpressedKey, VirtualKey secondKey)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.Table<KeyboardAction>().Where(x => x.MainKey == mainpressedKey && x.SecondKey == secondKey).FirstOrDefaultAsync();
        }

        public void Drop()
        {
            throw new NotImplementedException();
        }
    }
}
