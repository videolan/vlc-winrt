using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using SQLite;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.DataRepository
{
    public class TrackDataRepository : IDataRepository
    {
        private static readonly string _dbPath =
    Path.Combine(
    Windows.Storage.ApplicationData.Current.LocalFolder.Path,
    "mediavlc.sqlite");

        public TrackDataRepository()
        {
            Initialize();
        }

        public void Initialize()
        {
            using (var db = new SQLite.SQLiteConnection(_dbPath))
            {
                db.CreateTable<MusicLibraryVM.TrackItem>();
            }
        }

        public void Drop()
        {
            using (var db = new SQLite.SQLiteConnection(_dbPath))
            {
                db.DropTable<MusicLibraryVM.TrackItem>();
            }
        }

        public async Task<MusicLibraryVM.TrackItem> LoadTrack(int trackId)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<MusicLibraryVM.TrackItem>().Where(x => x.Id.Equals(trackId));
            var result = await query.ToListAsync();
            return result.FirstOrDefault();
        }

        public async Task<MusicLibraryVM.TrackItem> LoadTrack(int artistId, int albumId, string trackName)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<MusicLibraryVM.TrackItem>().Where(x => x.Name.Equals(trackName)).Where(x => x.ArtistId == artistId).Where(x => x.AlbumId == albumId);
            var result = await query.ToListAsync();
            return result.FirstOrDefault();
        }

        public
            async Task<ObservableCollection<MusicLibraryVM.TrackItem>> LoadTracksByAlbumId(int albumId)
        {

            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<MusicLibraryVM.TrackItem>().Where(x => x.AlbumId == albumId);
            var result = await query.ToListAsync();
            return new ObservableCollection<MusicLibraryVM.TrackItem>(result);
        }

        public Task Update(MusicLibraryVM.TrackItem track)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.UpdateAsync(track);
        }

        public async Task Add(MusicLibraryVM.TrackItem track)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<MusicLibraryVM.TrackItem>().Where(x => x.Path == track.Path);
            var result = await query.ToListAsync();
            if(result.Count == 0)
                connection.InsertAsync(track);
        }

        public async Task Remove(string folderPath)
        {
            // This will delete all the entries that are in folderPath and its subfolders
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<MusicLibraryVM.TrackItem>().Where(x => x.Path.Contains(folderPath));
            var result = await query.ToListAsync();
            foreach (MusicLibraryVM.TrackItem trackItem in result)
            {
                connection.DeleteAsync(trackItem);
            }

            // If all tracks for an album are deleted, then remove the album itself
            foreach (MusicLibraryVM.TrackItem trackItem in result)
            {
                MusicLibraryVM.AlbumItem album = await MusicLibraryVM._albumDataRepository.LoadAlbum(trackItem.AlbumId);
                if(album != null)
                    MusicLibraryVM._albumDataRepository.Remove(album);
            }
        }
    }
}
