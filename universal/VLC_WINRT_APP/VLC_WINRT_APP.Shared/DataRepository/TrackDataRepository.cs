using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using SQLite;
using VLC_WINRT_APP.Model.Music;
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
                db.CreateTable<TrackItem>();
            }
        }

        public void Drop()
        {
            using (var db = new SQLite.SQLiteConnection(_dbPath))
            {
                db.DropTable<TrackItem>();
            }
        }

        public async Task<TrackItem> LoadTrack(int trackId)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<TrackItem>().Where(x => x.Id.Equals(trackId));
            var result = await query.ToListAsync();
            return result.FirstOrDefault();
        }

        public async Task<TrackItem> LoadTrack(int artistId, int albumId, string trackName)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<TrackItem>().Where(x => x.Name.Equals(trackName)).Where(x => x.ArtistId == artistId).Where(x => x.AlbumId == albumId);
            var result = await query.ToListAsync();
            return result.FirstOrDefault();
        }

        public
            async Task<ObservableCollection<TrackItem>> LoadTracksByAlbumId(int albumId)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<TrackItem>().Where(x => x.AlbumId == albumId);
            var result = await query.ToListAsync();
            return new ObservableCollection<TrackItem>(result);
        }
        public
            async Task<ObservableCollection<TrackItem>> LoadTracksByArtistId(int artistId)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<TrackItem>().Where(x => x.ArtistId == artistId);
            var result = await query.ToListAsync();
            return new ObservableCollection<TrackItem>(result);
        }

        public async Task<ObservableCollection<TrackItem>> LoadTracks()
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<TrackItem>();
            var result = await query.ToListAsync();
            return new ObservableCollection<TrackItem>(result);
        }

        public Task Update(TrackItem track)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.UpdateAsync(track);
        }

        public async Task Add(TrackItem track)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<TrackItem>().Where(x => x.Path == track.Path);
            var result = await query.ToListAsync();
            if(result.Count == 0)
                await connection.InsertAsync(track);
        }

        public async Task Remove(string folderPath)
        {
            // TODO: None of this logic should live here. This is for "TrackDataRepository",
            // so it should not know about the other repositories.

            // This will delete all the entries that are in folderPath and its subfolders
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<TrackItem>().Where(x => x.Path.Contains(folderPath));
            var result = await query.ToListAsync();
            foreach (TrackItem trackItem in result)
            {
               await connection.DeleteAsync(trackItem);
            }

            // If all tracks for an album are deleted, then remove the album itself
            foreach (TrackItem trackItem in result)
            {
                AlbumItem album = await MusicLibraryVM._albumDataRepository.LoadAlbum(trackItem.AlbumId);
                if(album != null)
                    MusicLibraryVM._albumDataRepository.Remove(album);
            }

            // If all the albums for the artist are gone, remove the artist
            var firstTrack = result.FirstOrDefault();
            if (firstTrack == null)
            {
                return;
            }

            var albums = await MusicLibraryVM._albumDataRepository.LoadAlbumsFromId(firstTrack.ArtistId);
            if (albums != null && !albums.Any())
            {
                var artist = await MusicLibraryVM._artistDataRepository.LoadArtist(firstTrack.ArtistId);
                await MusicLibraryVM._artistDataRepository.Remove(artist);
            }


        }

    }
}
