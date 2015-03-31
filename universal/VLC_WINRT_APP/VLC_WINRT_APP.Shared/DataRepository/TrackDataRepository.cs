using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using SQLite;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels.MusicVM;
using WinRTXamlToolkit.IO.Serialization;
using System.Collections.Generic;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.DataRepository
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
            var db = new SQLite.SQLiteConnection(_dbPath);
            db.CreateTable<TrackItem>();
        }

        public void Drop()
        {
            using (var db = new SQLite.SQLiteConnection(_dbPath))
            {
                db.DropTable<TrackItem>();
            }
        }

        public async Task<bool> DoesTrackExist(string path)
        {
            bool b = false;
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<TrackItem>().Where(x => x.Path == path);
            b = await query.CountAsync() != 0;
            return b;
        }

        public async Task<TrackItem> LoadTrackByPath(string path)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<TrackItem>().Where(x => x.Path == path);
            if (await query.CountAsync() > 0)
            {
                return await query.FirstOrDefaultAsync();
            }
            return null;
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

        public async Task<List<TrackItem>> LoadTracksByAlbumId(int albumId)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<TrackItem>().Where(x => x.AlbumId == albumId);
            return await query.ToListAsync();
        }

        public async Task<string> GetFirstTrackPathByAlbumId(int albumId)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<TrackItem>().Where(x => x.AlbumId == albumId);
            var result = await query.FirstOrDefaultAsync();
            return result != null ? result.Path : null;
        }

        public async Task<List<TrackItem>> LoadTracksByArtistId(int artistId)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<TrackItem>().Where(x => x.ArtistId == artistId);
            return await query.ToListAsync();
        }

        public async Task<List<TrackItem>> LoadTracks()
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            var query = connection.Table<TrackItem>().OrderBy(x => x.Name);
            return await query.ToListAsync();
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
            if (result.Count == 0)
                await connection.InsertAsync(track);
        }

        public Task Remove(TrackItem track)
        {
            var connection = new SQLiteAsyncConnection(_dbPath);
            return connection.DeleteAsync(track);
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
                AlbumItem album = await Locator.MusicLibraryVM._albumDataRepository.LoadAlbum(trackItem.AlbumId);
                if (album != null)
                    Locator.MusicLibraryVM._albumDataRepository.Remove(album);
            }

            // If all the albums for the artist are gone, remove the artist
            var firstTrack = result.FirstOrDefault();
            if (firstTrack == null)
            {
                return;
            }

            var albums = await Locator.MusicLibraryVM._albumDataRepository.LoadAlbumsFromId(firstTrack.ArtistId);
            if (albums != null && !albums.Any())
            {
                var artist = await Locator.MusicLibraryVM._artistDataRepository.LoadArtist(firstTrack.ArtistId);
                await Locator.MusicLibraryVM._artistDataRepository.Remove(artist);
            }
        }

    }
}
