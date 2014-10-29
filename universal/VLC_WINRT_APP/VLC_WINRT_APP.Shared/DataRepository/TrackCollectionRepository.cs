using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using SQLite;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels.MusicVM;

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
                db.CreateTable<TrackCollectionItem>();
            }
        }
        public async Task<ObservableCollection<TrackCollectionItem>> LoadTrackCollections()
        {
            var connection = new SQLiteAsyncConnection(DbPath);

            return new ObservableCollection<TrackCollectionItem>(
               await connection.QueryAsync<TrackCollectionItem>(
                     "select * from TrackCollectionItem"));
        }

        public Task Add(TrackCollectionItem trackCollection)
        {
            var connection = new SQLiteAsyncConnection(DbPath);
            return connection.InsertAsync(trackCollection);
        }

        public async Task Remove(TrackCollectionItem trackCollection)
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
            throw new NotImplementedException();
        }
    }
}
