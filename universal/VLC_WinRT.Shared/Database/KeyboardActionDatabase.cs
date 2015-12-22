using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using SQLite;
using VLC_WinRT.Model;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Database
{
    public class KeyboardActionDatabase : IDatabase
    {
        private static readonly string DbPath = Strings.SettingsDatabase;

        public KeyboardActionDatabase()
        {
            Initialize();
        }

        public void Initialize()
        {
            try
            {
                using (var db = new SQLiteConnection(DbPath))
                {
                    db.CreateTable<KeyboardAction>();
                }
            }
            catch
            {
            }
        }

        public bool IsEmpty()
        {
            using (var c = new SQLiteConnection(DbPath))
                return !c.Table<KeyboardAction>().Any();
        }

        public void AddKeyboardActions(IEnumerable<KeyboardAction> keyboardActions)
        {
            using (var connection = new SQLiteConnection(DbPath))
            {
                connection.InsertAll(keyboardActions);
            }
        }

        public Task UpdateKeyboardAction(KeyboardAction keyboardAction)
        {
            //one shortcut per action, so only updates
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.UpdateAsync(keyboardAction);
        }

        public List<KeyboardAction> GetAllKeyboardActions()
        {
            using (var connection = new SQLiteConnection(DbPath))
            {
                var table = connection.Table<KeyboardAction>();
                var oc = table.ToList();
                return oc;
            }
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
